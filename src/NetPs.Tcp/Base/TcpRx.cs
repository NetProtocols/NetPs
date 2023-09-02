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
        protected TcpCore core { get; }
        protected byte[] bBuffer { get; private set; }
        public int nReceived { get; protected set; }
        private int nBuffersize { get; set; }
        private bool is_disposed = false;
        public bool Running => this.core.Receiving;
        protected TaskFactory Task { get; private set; }
        private ITcpReceive receive { get; set; }
        private IAsyncResult AsyncResult { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpRx"/> class.
        /// </summary>
        /// <param name="tcpCore">.</param>
        public TcpRx(TcpCore tcpCore)
        {
            this.Task = new TaskFactory();
            this.nReceived = 0;
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

        public virtual void WhenReceived(ITcpReceive tcp_receive)
        {
            this.receive = tcp_receive;
        }
        /// <summary>
        /// 开始接收.
        /// </summary>
        public virtual void StartReceive()
        {
            if (this.is_disposed) return;
            lock (this.core)
            {
                if (this.core.Receiving) return;
                this.core.Receiving = true;
            }

            this.restart_receive();
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
                this.core.Receiving = false;
            }
            if (AsyncResult != null)
            {
                this.core.Socket.EndReceive(AsyncResult);
                AsyncResult.AsyncWaitHandle.Close();
            }
            this.bBuffer = null;
        }

        /// <summary>
        /// 发送接收事件
        /// </summary>
        /// <param name="data"></param>
        public virtual void SendReceived(byte[] data)
        {
            if (data != null && data.Length > 0)
            {
                if (this.receive != null) receive.TcpReceive(data, this.core as TcpClient);
                if (this.Received != null) this.Received.Invoke(data);
            }
        }

        public virtual void OnRecevied()
        {
            var data = new byte[this.nReceived];
            Array.Copy(this.bBuffer, data, this.nReceived);
            SendReceived(data);
            Task.StartNew(restart_receive);
        }

        protected void restart_receive() => Task.StartNew(this.x_StartReceive);

        private void x_StartReceive()
        {
            try
            {
                if (this.is_disposed || !this.core.Actived) return;
                this.AsyncResult = this.core.Socket.BeginReceive(this.bBuffer, 0, this.nBuffersize, SocketFlags.None, this.ReceiveCallback, null);
                return;
            }
            //释放
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException e)
            {
                if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Read)) this.core.ThrowException(e);
            }

            if (!this.is_disposed) this.core.Lose();
        }
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                if (this.is_disposed || !this.core.Actived) return;
                this.nReceived = this.core.Socket.EndReceive(asyncResult);
                asyncResult.AsyncWaitHandle.Close();
                if (this.nReceived <= 0)
                {
                    //if (this.is_disposed) return;
                    if (this.core.Socket.Poll(1000, SelectMode.SelectRead))
                    {
                        // 接收到 FIN
                        this.core.FIN();
                        return;
                    }
                    this.core.Lose();
                    return;
                }
                if (this.is_disposed) return;
                this.OnRecevied();
                return;
            }
            //释放
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException e)
            {
                this.nReceived = 0;

                if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Read)) this.core.ThrowException(e);
            }
            if (!this.is_disposed) this.core.Lose();
        }

    }
}
