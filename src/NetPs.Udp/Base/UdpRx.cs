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
        private bool is_disposed = false;
        private bool has_address = false;
        private EndPoint remotePoint;
        private EndPoint receivedPoint;
        protected byte[] bBuffer { get; private set; }
        public int nReceived { get; protected set; }
        private int nBuffersize { get; set; }
        protected TaskFactory Task { get; set; }
        private IAsyncResult AsyncResult { get; set; }
        private IRxEvents events { get; set; }
        private IUdpRx udpRx { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpRx"/> class.
        /// </summary>
        /// <param name="udpCore">.</param>
        public UdpRx()
        {
            this.Task = new TaskFactory();
            this.nBuffersize = Consts.ReceiveBytes;
            this.bBuffer = new byte[this.nBuffersize];
            this.ReceivedObservable = Observable.FromEvent<ReveicedStreamHandler, UdpData>(handler => data => handler(data), evt => this.Received += evt, evt => this.Received -= evt);
        }

        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<UdpData> ReceivedObservable { get; protected set; }
        /// <summary>
        /// 接收数据.
        /// </summary>
        public virtual event ReveicedStreamHandler Received;
        public virtual event ReveicedNoBufferHandler NoBufferReceived;
        private event ReveicedNoBufferHandler InsideReceived;
        /// <summary>
        /// Gets 接送缓冲区大小.
        /// </summary>
        public virtual int ReceiveBufferSize => this.nBuffersize;
        public virtual byte[] Buffer => this.bBuffer;
        public virtual int ReceivedSize => this.nReceived;
        public virtual bool Running => this.Core.Receiving;
        public virtual IPEndPoint RemoteAddress => this.remotePoint as IPEndPoint;

        /// <summary>
        /// 开始接收.
        /// </summary>
        public virtual void StartReceive()
        {
            if (this.is_disposed) return;
            if (this.udpRx != null) return;
            lock (this.Core)
            {
                if (this.Core.Receiving) return;
                this.Core.Receiving = true;
            }
            if (this.remotePoint == null)
            {
                this.remotePoint = Core.CreateIPAny();
            }
            if (this.receivedPoint == null)
            {
                this.receivedPoint = Core.CreateIPAny();
            }
            this.events?.OnReceiving(this);
            this.x_StartReveice();
        }
        public virtual void SetRemoteAddress(IPEndPoint address)
        {
            this.remotePoint = address;
            if (!address.Address.IsAny())
            {
                this.has_address = true;
            }
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
            if (this.udpRx != null)
            {
                if (udpRx is UdpRx udp_rx)
                {
                    udp_rx.InsideReceived -= UdpRx_NoBufferReceived;
                }
                this.udpRx = null;
            }
            this.events?.OnDisposed(this);
            this.bBuffer = null;
        }

        public virtual void SendReceived(byte[] data, IPEndPoint address)
        {
            if (this.Received != null && data != null && data.Length > 0)
            {
                this.Received.Invoke(new UdpData { Data = data, IP = address });
            }
        }

        protected virtual void OnReceived(byte[] buffer, int length, IPEndPoint address)
        {
            var data = new byte[length];
            Array.Copy(buffer, 0, data, 0, this.nReceived);
            SendReceived(data, address);
            this.restart_receive();
        }

        protected void restart_receive()
        {
            if (this.udpRx != null)
            {
                return;
            }
            Task.StartNew(this.x_StartReveice);
        }
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
        private void SendReceivedData(byte[] buffer, int length, IPEndPoint address)
        {
            if (NoBufferReceived != null && NoBufferReceived.Invoke(buffer, length, address))
            {
                this.restart_receive();
                return;
            }
            if (InsideReceived != null && InsideReceived.Invoke(buffer, length, address))
            {
                this.restart_receive();
                return;
            }
            this.events?.OnReceived(this);
            this.OnReceived(buffer, length, address);
        }
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            this.AsyncResult = null;
            try
            {
                this.nReceived = this.Core.EndReceiveFrom(asyncResult, ref receivedPoint);
                if (this.nReceived > 0 && (!has_address || this.RemoteAddress.Equals(receivedPoint)))
                {
                    this.SendReceivedData(Buffer, this.nReceived, receivedPoint as IPEndPoint);
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
        public void UseRx(IUdpRx rx)
        {
            this.udpRx = rx;
            if (udpRx is UdpRx udp_rx)
            {
                udp_rx.InsideReceived += UdpRx_NoBufferReceived;
            }
        }

        private bool UdpRx_NoBufferReceived(byte[] buffer, int length, IPEndPoint address)
        {
            if (!address.Equals(this.remotePoint)) return false;
            this.SendReceivedData(buffer, length, address);
            return true;
        }

    }
}
