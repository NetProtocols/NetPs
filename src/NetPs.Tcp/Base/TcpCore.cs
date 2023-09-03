namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    public delegate void TcpConfigFunction(TcpCore tcpCore);

    /// <summary>
    /// Tcp基类
    /// </summary>
    public class TcpCore : SocketCore
    {
        private TcpConfigFunction tcp_config { get; set; }
        private IAsyncResult AsyncResult { get; set; }
        private bool is_disposed = false;
        private bool is_connecting = false;
        private bool is_connected = false;
        public TcpCore()
        {
            this.tcp_config = null;
            construct();
        }
        public TcpCore(TcpConfigFunction tcp_config)
        {
            this.tcp_config = tcp_config;
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
        public override bool Actived => base.Actived && (this.Socket?.Connected ?? false);

        /// <summary>
        /// 可以结束
        /// </summary>
        public override bool CanEnd => (this.Socket.Connected == this.Socket.Blocking) || (this.Socket.Connected && !this.Socket.Blocking);
        /// <summary>
        /// 可以Poll
        /// </summary>
        public override bool CanFIN => !(this.Socket.Blocking && this.Socket.Connected);
        /// <summary>
        /// 可以开始
        /// </summary>
        public override bool CanBegin => !(!(this.Socket?.Blocking ?? false) && !(this.Socket?.Connected ?? false));

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets 正在连接.
        /// </summary>
        public virtual bool IsConnecting => this.is_connecting;

        public virtual bool IsConnected => this.is_connected;

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

        /// <summary>
        /// 创建新连接.
        /// </summary>
        /// <param name="address">.</param>
        public virtual async Task<bool> ConnectAsync(SocketUri address)
        {
            if (this.connect_pre(address))
            {
                return await this.connect_task(this.ConnectTimeout);
            }
            return false;
        }
        public virtual async Task<bool> ConnectAsync(string address) => await this.ConnectAsync(new SocketUri(address));
        public virtual void Connect(SocketUri address)
        {
            if (this.connect_pre(address))
            {
                this.just_start_connect();
            }
        }
        public virtual void Connect(string address) => this.Connect(new SocketUri(address));
        private bool connect_pre(SocketUri address)
        {
            if (address != null)
            {
                lock (this)
                {
                    if (this.is_connecting)
                    {
                        if (address.Equal(this.IPEndPoint)) return false;
                        throw new NetPsSocketException(SocketErrorCode.ConnectionRefused, "is connecting");
                    }
                    this.is_connecting = true;
                }
                this.Address = address;
                this.IPEndPoint = new IPEndPoint(address.IP, address.Port);
                this.Socket = new Socket(address.IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                this.Socket.Blocking = false;
                this.OnConfiguration();
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
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            if (this.Socket != null)
            {
                if (CanFIN)
                {
                    try
                    {
                        //主动关闭的必要：发送 FIN 包
                        this.Socket.Shutdown(how);
                    }
                    catch (SocketException) { }
                }
            }
            if (!base.IsClosed) this.Lose();
        }

        public virtual void TcpConfigure(TcpCore core)
        {
            if (tcp_config != null) tcp_config.Invoke(core);
        }

        /// <summary>
        /// 当链接开始.
        /// </summary>
        protected virtual void OnConnected() { }
        /// <summary>
        /// 当socket配置时
        /// </summary>
        protected virtual void OnConfiguration() { }
        /// <summary>
        /// 当连接结束
        /// </summary>
        protected virtual void OnDisconnected() { }
        protected override void OnClosed() { }
        protected override void OnLosed()
        {
            this.OnDisconnected();
            DisConnected?.Invoke(this.IPEndPoint);
        }

        public override void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            if (this.AsyncResult != null)
            {
                this.Socket.EndConnect(this.AsyncResult);
                this.AsyncResult.AsyncWaitHandle.Close();
            }
            base.Dispose();
        }

        private void just_start_connect()
        {
            this.start_connect();
        }

        private async Task<bool> connect_task(int timeout)
        {
            if (this.is_disposed) return false;
            try
            {
                var rep = this.ConnectedObservable.Timeout(TimeSpan.FromMilliseconds(timeout)).FirstOrDefaultAsync();
                var task = rep.GetAwaiter();
                this.just_start_connect();
                var o = await task;
                if (this.is_disposed) return false;
                if (o == this)
                {
                    return true;
                }
            }
            catch (TimeoutException) { }
            try
            {
                //超时释放
                if (this.AsyncResult != null)
                {
                    this.Socket.EndConnect(this.AsyncResult);
                    this.AsyncResult.AsyncWaitHandle.Close();
                }
            }
            catch (NullReferenceException) { }
            catch (ObjectDisposedException) { }
            catch (SocketException) { }
            tell_disconnected();
            return false;
        }

        protected virtual void start_connect()
        {
            if (this.is_disposed) return;
            try
            {
                AsyncResult = this.Socket.BeginConnect(this.IPEndPoint, this.connect_callback, null);
                return;
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            //防止 SocketErr: 10045
            catch (SocketException) { }
            this.tell_disconnected();
        }
        private void connect_callback(IAsyncResult asyncResult)
        {
            if (this.is_disposed) return;
            try
            {
                this.Socket.EndConnect(asyncResult);
                asyncResult.AsyncWaitHandle.Close();
                this.tell_connected();
                return;
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException)
            {
            }
            this.tell_disconnected();
        }
        private void tell_connected()
        {
            if (this.is_disposed) return;
            this.is_connecting = false;
            this.is_connected = true;
            this.OnConnected();
            this.Connected?.Invoke(this);
        }

        private void tell_disconnected()
        {
            if (this.is_disposed) return;
            this.is_connecting = false;
            this.is_connected = false;
            this.OnDisconnected();
            this.DisConnected?.Invoke(this);
        }
    }
}
