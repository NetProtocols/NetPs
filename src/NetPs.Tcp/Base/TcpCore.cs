namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;

    /// <summary>
    /// .
    /// </summary>
    public class TcpCore : SocketCore, ITcpConfig
    {
        private readonly Action<TcpCore> tcp_config;
        private ITcpConfig X_TcpConfig { get; }
        public TcpCore(Action<TcpCore> tcp_config)
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

        public virtual Action<TcpCore> Tcp_config => this.tcp_config;
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
                    using (this.Socket)
                    {
                        this.Socket.Close();
                    }
                }

                this.Connecting = true;
                this.IPEndPoint = new IPEndPoint(address.IP, address.Port);
                this.Socket = new Socket(address.IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                X_TcpConfig.TcpConfigure(this);
                this.StartConnect(this.ConnectTimeout);
            }
        }

        public void TcpConfigure(TcpCore core)
        {
            if (tcp_config != null) tcp_config.Invoke(core);
        }
    }
}
