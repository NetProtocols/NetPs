namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public delegate void TcpConfigFunction(TcpCore tcpCore);

    /// <summary>
    /// Tcp基类
    /// </summary>
    public class TcpCore : SocketCore
    {
        private bool is_disposed = false;
        private bool is_connecting = false;
        private bool is_connected = false;
        private ManualResetEvent manualResetEvent = new ManualResetEvent(false);
        private TcpConfigFunction tcp_config { get; set; }
        private IAsyncResult AsyncResult { get; set; }
        public TcpCore() : base()
        {
            construct();
        }
        public TcpCore(TcpConfigFunction tcp_config) : base()
        {
            this.tcp_config = tcp_config;
            construct();
        }
        public TcpCore(Socket socket) : base(socket)
        {
            construct();
        }
        private void construct()
        {
            this.ConnectedObservable = Observable.FromEvent<SateChangeHandle, object>(handler => o => handler(o), evt => this.Connected += evt, evt => this.Connected -= evt);
            this.LoseConnectedObservable = Observable.FromEvent<SateChangeHandle, object>(handler => o => handler(o), evt => this.DisConnected += evt, evt => this.DisConnected -= evt);
        }

        /// <summary>
        /// 连接.
        /// </summary>
        public virtual event SateChangeHandle Connected;

        /// <summary>
        /// 丢失连接.
        /// </summary>
        public virtual event SateChangeHandle DisConnected;

        /// <summary>
        /// Gets or sets 连接超时毫秒(ms).
        /// </summary>
        public virtual int ConnectTimeout { get; set; } = 3600;

        /// <summary>
        /// Gets or sets a value indicating whether 正在接收.
        /// </summary>
        public virtual bool Receiving { get; set; }

        /// <summary>
        /// 活动状态
        /// </summary>
        public override bool Actived => base.Actived && (this.IsServer || this.IsConnected && !this.IsShutdown);
        public virtual bool IsServer => false;
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets 正在连接.
        /// </summary>
        public virtual bool IsConnecting => this.is_connecting;
        public virtual bool IsConnected => (this.IsReference || this.is_connected) && Socket.Connected;
        public override bool IsClosed => base.IsClosed || this.IsShutdown;

        /// <summary>
        /// Gets or sets 连接.
        /// </summary>
        public virtual IObservable<object> ConnectedObservable { get; protected set; }

        /// <summary>
        /// Gets or sets 丢失连接.
        /// </summary>
        public virtual IObservable<object> LoseConnectedObservable { get; protected set; }

        //开始监听Accept
        public virtual void Listen(int backlog)
        {
            this.Socket.Listen(backlog);
            base.to_opened();
        }
        public virtual bool Connect(ISocketUri address)
        {
            if (this.connect_pre(address))
            {
                try
                {
                    return this.connect_task(this.ConnectTimeout);
                }
                catch when (this.IsClosed) { Debug.Assert(false); }
                catch (Exception e) { this.ThrowException(e); }
            }
            return false;
        }
        public virtual bool Connect(string address) => this.Connect(new InsideSocketUri(InsideSocketUri.UriSchemeTCP, address));
        private bool connect_pre(ISocketUri address)
        {
            if (address != null)
            {
                if (address.IsAny())
                {
                    address = address.ToLoopback();
                }
                lock (this)
                {
                    if (this.is_connecting)
                    {
                        if (address.Equal(this.RemoteIPEndPoint)) return false;
                        throw new NetPsSocketException(SocketErrorCode.ConnectionRefused, "is connecting");
                    }
                    this.is_connecting = true;
                }
                this.RemoteAddress = address;
                this.RemoteIPEndPoint = new IPEndPoint(address.IP, address.Port);
                if (Address == null)
                {
                    this.Address = address;
                    this.OnConfiguration();
                }
                else
                {
                    this.Bind(this.Address);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 发送TCP FIN
        /// </summary>
        public virtual void FIN() => Shutdown(SocketShutdown.Both);

        /// <summary>
        /// 发送TCP FIN
        /// </summary>
        /// <remarks>
        /// 关闭告知，此方法可能触发Socket Losed。
        /// </remarks>
        public virtual void Shutdown(SocketShutdown how)
        {
            //主动关闭的必要：发送 FIN 包
            this.socket_shutdown(how);
            if (!base.IsClosed) this.Lose();
        }

        public override void Bind()
        {
            this.OnConfiguration();
            base.Bind();
        }

        /// <summary>
        /// 当链接开始.
        /// </summary>
        protected virtual void OnConnected() { }
        /// <summary>
        /// 当socket配置时
        /// </summary>
        protected virtual void OnConfiguration()
        {
            this.SetSocket(new_socket());
            //this.Socket.Blocking = false;
            if (tcp_config != null) tcp_config.Invoke(this);
        }
        /// <summary>
        /// 当连接结束
        /// </summary>
        protected virtual void OnDisconnected()
        {
            DisConnected?.Invoke(this);
        }

        public override void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            this.manualResetEvent.Set();
            this.manualResetEvent.Close();
            base.Dispose();
        }
        private bool connect_task(int timeout)
        {
            if (this.is_disposed) return false;
            try
            {
                manualResetEvent.Reset();
                this.start_connect(false);
                if (manualResetEvent.WaitOne(timeout, false))
                {
                    if (!this.is_connected || this.is_disposed) return false;
                    return true;
                }
            }
            catch when (this.IsClosed) { Debug.Assert(false); }
            catch (Exception e) { this.ThrowException(e); }
            if (!this.is_disposed)
            {
                tell_disconnected();
            }
            return false;
        }

        protected virtual void start_connect(bool wait = true)
        {
            if (this.is_disposed) return;
            if (! to_opened()) return;
            try
            {
                AsyncResult = this.BeginConnect(this.RemoteIPEndPoint, this.connect_callback);
                if (AsyncResult != null)
                {
                    if (wait) AsyncResult.Wait();
                    return;
                }
            }
            //防止 SocketErr: 10045
            catch when (this.IsClosed) { Debug.Assert(false); }
            catch (Exception e) { this.ThrowException(e); }
            AsyncResult = null;
            this.tell_disconnected();
        }
        private void connect_callback(IAsyncResult asyncResult)
        {
            AsyncResult = null;
            try
            {
                this.EndConnect(asyncResult);
                if (!this.IsClosed && this.Socket.Connected)
                {
                    this.tell_connected();
                    return;
                }
            }
            catch when (this.IsClosed) { Debug.Assert(false); }
            catch (Exception e) { this.ThrowException(e); }
            if (!this.IsClosed) this.tell_disconnected();
        }
        private void tell_connected()
        {
            if (this.is_disposed) return;
            lock (this)
            {
                this.is_connecting = false;
                if (this.is_connected) return;
                this.is_connected = true;
            }
            this.manualResetEvent.Set();
            this.OnConnected();
            this.Connected?.Invoke(this);
        }

        private void tell_disconnected()
        {
            if (this.is_disposed) return;
            lock (this)
            {
                if (!this.is_connecting) return;
                this.is_connecting = false;
                this.is_connected = false;
            }
            this.OnDisconnected();
        }

        protected override void OnClosed()
        {
        }

        protected override void OnLosed()
        {
        }
    }
}
