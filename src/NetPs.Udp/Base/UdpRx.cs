namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    public class UdpRx : BindUdpCore, IDisposable, IUdpRx
    {
        protected byte[] bBuffer { get; private set; }
        public int nReceived { get; protected set; }
        private int nBuffersize { get; set; }
        private bool is_disposed = false;
        private EndPoint remotePoint;
        protected TaskFactory Task { get; set; }
        private IAsyncResult AsyncResult { get; set; }
        private IRxEvents events { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpRx"/> class.
        /// </summary>
        /// <param name="udpCore">.</param>
        public UdpRx()
        {
            this.Task = new TaskFactory();
            this.nBuffersize = Consts.ReceiveBytes;
            this.bBuffer = new byte[this.nBuffersize];
            this.ReceicedObservable = Observable.FromEvent<ReveicedStreamHandler, UdpData>(handler => data => handler(data), evt => this.Received += evt, evt => this.Received -= evt);
        }

        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<UdpData> ReceicedObservable { get; protected set; }

        /// <summary>
        /// 接收数据.
        /// </summary>
        public virtual event ReveicedStreamHandler Received;

        /// <summary>
        /// Gets 接送缓冲区大小.
        /// </summary>
        public virtual int ReceiveBufferSize => this.nBuffersize;
        public byte[] Buffer => this.bBuffer;
        public int ReceivedSize => this.nReceived;
        public bool Running => this.Core.Receiving;
        public IPEndPoint RemoteAddress => this.remotePoint as IPEndPoint;

        /// <summary>
        /// 开始接收.
        /// </summary>
        public virtual void StartReveice()
        {
            if (this.is_disposed) return;
            lock (this.Core)
            {
                if (this.Core.Receiving) return;
                this.Core.Receiving = true;
            }
            if (this.remotePoint == null)
            {
                this.remotePoint = Core.CreateIPAny();
            }
            this.events?.OnReceiving(this);
            this.x_StartReveice();
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock(this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
                if (this.Core != null) this.Core.Receiving = false;
            }

            this.events?.OnDisposed(this);
            this.bBuffer = null;
            
        }

        public virtual void SendReceived(byte[] data)
        {
            if (this.Received != null && data != null && data.Length > 0)
            {
                this.Received.Invoke(new UdpData { Data = data, IP = this.remotePoint as IPEndPoint });
            }
        }

        protected virtual void OnReceived()
        {
            var data = new byte[this.nReceived];
            Array.Copy(this.bBuffer, 0, data, 0, this.nReceived);
            SendReceived(data);
            this.restart_receive();
        }

        protected void restart_receive() => Task.StartNew(this.x_StartReveice);

        private void x_StartReveice()
        {
            if (this.is_disposed || !this.Core.Actived) return;
            try
            {
                AsyncResult = this.Core.BeginReceiveFrom(this.bBuffer, 0, this.nBuffersize, ref remotePoint, this.ReceiveCallback);
                if (AsyncResult != null)
                {
                    AsyncResult.Wait();
                    return;
                }
            }
            //释放
            catch when (this.Core.IsClosed) { Debug.Assert(false); }
            catch (SocketException)
            {
                if (!this.is_disposed)
                {
                    restart_receive(); //忽略 客户端连接错误
                    return;
                }
            }
            catch (Exception e)
            {
                this.Core.ThrowException(e);
            }

            this.Core.Receiving = false;
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            this.AsyncResult = null;
            try
            {
                this.nReceived = this.Core.EndReceiveFrom(asyncResult, ref this.remotePoint);
                if (this.nReceived > 0)
                {
                    this.events?.OnReceived(this);
                    this.OnReceived();
                    return;
                }
                else if(!this.is_disposed)
                {
                    this.restart_receive();
                    return;
                }
            }
            //释放
            catch when (this.Core.IsClosed) { Debug.Assert(false); }
            catch (SocketException)
            {
                this.nReceived = -1;
                if (!this.is_disposed)
                {
                    restart_receive(); //忽略 客户端连接错误
                    return;
                }
            }
            catch (Exception e) { this.Core.ThrowException(e); }

            this.Core.Receiving = false;
        }

        public void BindEvents(IRxEvents events)
        {
            this.events = events;
        }
    }
}
