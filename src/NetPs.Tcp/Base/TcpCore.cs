namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;

    public delegate void TcpConfigFunction(TcpCore tcpCore);

    /// <summary>
    /// .
    /// </summary>
    public class TcpCore : SocketCore, ITcpConfig
    {
        private TcpConfigFunction tcp_config { get; }
        private ITcpConfig X_TcpConfig { get; }
        public TcpCore()
        {
            this.tcp_config = null;
            X_TcpConfig = this;
            construct();
        }
        public TcpCore(TcpConfigFunction tcp_config)
        {
            this.tcp_config = tcp_config;
            X_TcpConfig = this;
            construct();
        }

        public TcpCore(ITcpConfig config)
        {
            X_TcpConfig = config;
            construct();
        }

        private void construct()
        {
            this.ConnectedObservable = Observable.FromEvent<SateChangeHandler, IPEndPoint>(handler => ip => handler(ip), evt => this.Connected += evt, evt => this.Connected -= evt);
            this.LoseConnectedObservable = Observable.FromEvent<SateChangeHandler, IPEndPoint>(handler => ip => handler(ip), evt => this.DisConnected += evt, evt => this.DisConnected -= evt);
        }

        /// <summary>
        /// Gets or sets a value indicating whether 正在接收.
        /// </summary>
        public virtual bool Receiving { get; set; }

        /// <summary>
        /// Gets or sets 连接.
        /// </summary>
        public virtual IObservable<IPEndPoint> ConnectedObservable { get; protected set; }

        /// <summary>
        /// Gets or sets 丢失连接.
        /// </summary>
        public virtual IObservable<IPEndPoint> LoseConnectedObservable { get; protected set; }

        /// <summary>
        /// 连接到..
        /// </summary>
        /// <param name="address">地址.</param>
        public virtual void Connect(string address)
        {
            this.Connect(new SocketUri(address));
        }

        /// <summary>
        /// 创建新连接.
        /// </summary>
        /// <param name="address">.</param>
        public virtual void Connect(SocketUri address)
        {
            if (address != null)
            {
                this.Address = address;

                if (this.Connecting)
                {
                    if (this.IPEndPoint.Address == address.IP && this.IPEndPoint.Port == address.Port)
                    {
                        return;
                    }

                    this.Connecting = false;
                    if (this.Socket != null && this.Socket is IDisposable o) o.Dispose();
                }

                this.Connecting = true;
                this.IPEndPoint = new IPEndPoint(address.IP, address.Port);
                this.Socket = new Socket(address.IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                this.Socket.Blocking = false;
                X_TcpConfig.TcpConfigure(this);
                this.StartConnect(this.ConnectTimeout);
            }
        }

        public virtual void TcpConfigure(TcpCore core)
        {
            if (tcp_config != null) tcp_config.Invoke(core);
        }

        protected override void OnConnected()
        {
        }

        protected override void OnClosed()
        {
        }
    }
}
