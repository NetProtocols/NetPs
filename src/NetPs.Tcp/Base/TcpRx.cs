namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
#if NET35_CF
    using Array = System.Array2;
#endif

    /// <summary>
    /// tcp 接收控制.
    /// </summary>
    public class TcpRx : IDisposable
    {
        private readonly TcpCore core;

        private byte[] bBuffer { get; set; }

        private int nReceived { get; set; }

        private int nBuffersize { get; set; }

        private bool isDisposed { get; set; }
        private int zero_receive_times { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpRx"/> class.
        /// </summary>
        /// <param name="tcpCore">.</param>
        public TcpRx(TcpCore tcpCore)
        {
            this.isDisposed = false;
            this.zero_receive_times = 0;
            this.core = tcpCore;
            this.nBuffersize = Consts.ReceiveBytes;
            this.bBuffer = new byte[this.nBuffersize];
            this.ReceivedObservable = Observable.FromEvent<ReceivedStreamHandler, byte[]>(handler => buffer => handler(buffer), evt => this.Received += evt, evt => this.Received -= evt);
        }

        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<byte[]> ReceivedObservable { get; protected set; }

        /// <summary>
        /// 接收流.
        /// </summary>
        /// <param name="stream">流.</param>
        public delegate void ReceivedStreamHandler(byte[] stream);

        /// <summary>
        /// 接收数据.
        /// </summary>
        public virtual event ReceivedStreamHandler Received;

        /// <summary>
        /// Gets 接送缓冲区大小.
        /// </summary>
        public virtual int ReceiveBufferSize => this.nBuffersize;


        /// <summary>
        /// 开始接收.
        /// </summary>
        public virtual void StartReceive()
        {
            this.core.Receiving = true;
            this.x_StartReveice();
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            this.isDisposed = true;
            this.Received = null;
            this.core.Receiving = false;
        }

        private void x_StartReveice()
        {
            if (this.isDisposed || !this.core.Actived)
            {
                this.core.OnLoseConnected();
                return;
            }

            try
            {
                this.core.Socket?.BeginReceive(this.bBuffer, 0, this.nBuffersize, SocketFlags.None, this.ReceiveCallback, this.core.Socket);
            }
            catch (Exception e)
            {
                if (e is SocketException socket_e)
                {
                    var ex = new NetPsSocketException(socket_e, this.core, NetPsSocketExceptionSource.Read);
                    if (!ex.Handled) this.core.ThrowException(ex);
                }
                x_StartReveice();
            }
        }
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            if (this.isDisposed)
            {
                return;
            }

            try
            {
                var client = (Socket)asyncResult.AsyncState;
                this.nReceived = client.EndReceive(asyncResult);
                //asyncResult.AsyncWaitHandle.Close();
            }
            catch (Exception e)
            {
                this.nReceived = -1;
                if (e is SocketException exception)
                {
                    var ex = new NetPsSocketException(exception, this.core, NetPsSocketExceptionSource.Read);
                    if (!ex.Handled)
                    {
                        this.core.ThrowException(e);
                    }
                }
            }

            if (this.nReceived > 0)
            {
                zero_receive_times = 0;
                var buffer = new byte[this.nReceived];
                Array.Copy(this.bBuffer, buffer, this.nReceived);
                Array.Clear(this.bBuffer, 0, this.nReceived);
                this.Received?.Invoke(buffer);
            }
            else if (zero_receive_times++ > 2)
            {
                this.core.OnLoseConnected();
            }

            this.x_StartReveice();
            asyncResult.AsyncWaitHandle.Close();
        }
    }
}
