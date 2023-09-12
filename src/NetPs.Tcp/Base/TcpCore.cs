﻿namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
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
        private TcpConfigFunction tcp_config { get; set; }
        private CancellationToken CancellationToken { get; set; }
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
        public virtual bool CanEnd => (this.Socket.Connected == this.Socket.Blocking) || (this.Socket.Connected && !this.Socket.Blocking);
        /// <summary>
        /// 可以Poll
        /// </summary>
        public virtual bool CanFIN => !(this.Socket.Blocking && this.Socket.Connected);
        /// <summary>
        /// 可以开始
        /// </summary>
        public virtual bool CanBegin => !(!(this.Socket?.Blocking ?? false) && !(this.Socket?.Connected ?? false));

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets 正在连接.
        /// </summary>
        public virtual bool IsConnecting => this.is_connecting;

        public virtual bool IsConnected => this.is_connected;

        /// <summary>
        /// 连接的地址
        /// </summary>
        public virtual SocketUri RemoteAddress { get; protected set; }

        /// <summary>
        /// 连接的地址
        /// </summary>
        public virtual IPEndPoint RemoteIPEndPoint { get; protected set; }

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
            if (this.is_disposed) return;
            if (this.Socket != null)
            {
                if (CanFIN)
                {
                    try
                    {
                        //主动关闭的必要：发送 FIN 包
                        this.Socket.Shutdown(how);
                    }
                    catch (ObjectDisposedException) { }
                    catch (SocketException) { }
                }
            }
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
            this.Socket = new Socket(this.Address.IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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
            if (this.CancellationToken != null)
            {
                this.CancellationToken.WaitHandle.Close();
            }
            if (this.AsyncResult != null)
            {
                SocketCore.WaitHandle(AsyncResult, () =>
                {
                    this.Socket.EndConnect(AsyncResult);
                });
                this.AsyncResult = null;
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
                if (CancellationToken == null) CancellationToken = new CancellationToken();
                //var rep = this.ConnectedObservable.Timeout(TimeSpan.FromMilliseconds(timeout)).FirstOrDefaultAsync();
                //var task = rep.GetAwaiter();
                this.just_start_connect();
                //var o = await task;
                if (!this.CancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(timeout, CancellationToken);
                }
                if (this.is_connected)
                {
                    return true;
                }
                if (this.is_disposed) return false;
            }
            catch (TimeoutException) { }
            //超时释放
            if (this.AsyncResult != null)
            {
                SocketCore.WaitHandle(AsyncResult, () =>
                {
                    if (this.CanEnd)
                    {
                        this.Socket.EndConnect(AsyncResult);
                    }
                });
                this.AsyncResult = null;
            }
            tell_disconnected();
            return false;
        }

        protected virtual void start_connect()
        {
            if (this.is_disposed) return;
            try
            {
                to_opened();
                AsyncResult = this.Socket.BeginConnect(this.RemoteIPEndPoint, this.connect_callback, null);
                return;
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            //防止 SocketErr: 10045
            catch (SocketException) { }
            AsyncResult = null;
            this.tell_disconnected();
        }
        private void connect_callback(IAsyncResult asyncResult)
        {
            AsyncResult = null;
            if (this.is_disposed) return;
            try
            {
                this.Socket.EndConnect(asyncResult);
                asyncResult.AsyncWaitHandle.Close();
                if (this.Socket.Connected) this.tell_connected();
                return;
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException) { }
            this.tell_disconnected();
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
            this.OnConnected();
            CancellationToken.WaitHandle.Close();
            CancellationToken = new CancellationToken();
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

        public virtual void FastOpen()
        {
            this.Socket.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)15, true);
        }
    }
}
