namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net.Sockets;
    using System.Reactive.Linq;
#if NET35_CF
    using Array = System.Array2;
#endif

    /// <summary>
    /// tcp 接收控制.
    /// </summary>
    public class TcpRx : IDisposable
    {
        private readonly TcpCore core;

        private byte[] bBuffer;

        private int nReceived;

        private int nBuffersize;

        private bool isDisposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpRx"/> class.
        /// </summary>
        /// <param name="tcpCore">.</param>
        public TcpRx(TcpCore tcpCore)
        {
            this.core = tcpCore;
            this.nBuffersize = Consts.ReceiveBytes;
            this.bBuffer = new byte[this.nBuffersize];
            this.ReceivedObservable = Observable.FromEvent<ReveicedStreamHandler, byte[]>(handler => buffer => handler(buffer), evt => this.Reveiced += evt, evt => this.Reveiced -= evt);
        }

        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<byte[]> ReceivedObservable { get; protected set; }

        /// <summary>
        /// 接收流.
        /// </summary>
        /// <param name="stream">流.</param>
        public delegate void ReveicedStreamHandler(byte[] stream);

        /// <summary>
        /// 接收数据.
        /// </summary>
        public virtual event ReveicedStreamHandler Reveiced;

        /// <summary>
        /// Gets 接送缓冲区大小.
        /// </summary>
        public virtual int ReceiveBufferSize => this.nBuffersize;

        private bool actived => !this.core?.Socket?.Connected ?? false;

        /// <summary>
        /// 开始接收.
        /// </summary>
        public virtual void StartReveice()
        {
            this._StartReveice();
            this.core.Receiving = true;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            this.isDisposed = true;
            this.Reveiced = null;
            this.core.Receiving = false;
        }

        private void _StartReveice()
        {
            if (this.isDisposed)
            {
                return;
            }

            if (this.core.Actived)
            {
                try
                {
                    this.core.Socket?.BeginReceive(this.bBuffer, 0, this.nBuffersize, SocketFlags.None, this.ReceiveCallback, this.core.Socket);
                }
                catch (SocketException e)
                {
                    var ex = new NetPsSocketException(e, this.core);
                    if (!ex.Handled)
                    {
                        throw ex;
                    }
                }
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
                try
                {
                    this.nReceived = client.EndReceive(asyncResult);
                    asyncResult.AsyncWaitHandle.Close();
                }
                catch (SocketException e)
                {
                    this.nReceived = -1;
                    var ex = new NetPsSocketException(e, this.core);
                    if (!ex.Handled)
                    {
                        this.core.ThrowException(e);
                    }
                }

                if (this.nReceived > 0)
                {
                    var buffer = new byte[this.nReceived];
                    Array.Copy(this.bBuffer, buffer, this.nReceived);
                    Array.Clear(this.bBuffer, 0, this.nReceived);
                    this.Reveiced?.Invoke(buffer);
                }
                else if (!this.actived)
                {
                    this.core.OnLoseConnected();
                    return;
                }

                this._StartReveice();
            }
            catch (Exception e)
            {
                this.core.ThrowException(e);
            }
        }
    }
}
