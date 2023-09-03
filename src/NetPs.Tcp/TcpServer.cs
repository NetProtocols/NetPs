namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;

    public delegate void TcpAcceptedFunction(TcpServer tcpServer, TcpClient tcpClient);
    public delegate void TcpClosedFunction();
    public delegate void TcpClientLosedFunction(TcpClient tcpClient);
    /// <summary>
    /// Tcp server.
    /// </summary>
    public class TcpServer : TcpCore, IDisposable, ISocketLose, ITcpServer
    {
        private bool alive = false;
        private TcpAcceptedFunction accepted_function { get; set; }
        private TcpClosedFunction closed_function { get; set; }
        private TcpClientLosedFunction losed_function { get; set; }
        private ITcpServerEvents events { get; set; }
        public TcpServer(TcpConfigFunction tcp_config = null) : base(tcp_config)
        {
            construct();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        public TcpServer(TcpAcceptedFunction accepted, TcpConfigFunction tcp_config = null) : base(tcp_config)
        {
            construct();
            this.accepted_function = accepted;
            this.Ax.Accepted += Ax_Accepted;
        }

        public TcpServer(ITcpServerConfig serverConfig) : base(serverConfig.OnConfiguration)
        {
            construct();
            this.Disposables.Add(this.AcceptObservable.Subscribe(s =>
            {
                var client = new TcpClient(s, this);
                if (serverConfig.TcpAccept(this, client))
                {
                    if (client.Actived)
                    {
                        client.WhenLoseConnected(this);
                        client.Rx.WhenReceived(serverConfig);
                        lock (this.Connects) { this.Connects.Add(client); }
                        return;
                        //client.StartReceive(serverConfig);
                    }
                }
                //无效客户端
                client.Dispose();
            }));
            this.Run(serverConfig.BandAddress);
        }

        private void construct()
        {
            this.Ax = new TcpAx(this);
            this.Connects = new List<TcpClient>(); // 65535-1024= 64571
        }

        /// <summary>
        /// 运行状态
        /// </summary>
        public virtual bool Alive => this.alive;

        /// <summary>
        /// Gets or sets 接受.
        /// </summary>
        public virtual IObservable<Socket> AcceptObservable => this.Ax.AcceptObservable;

        /// <summary>
        /// Gets or sets 服务.
        /// </summary>
        public virtual TcpAx Ax { get; protected set; }

        /// <inheritdoc/>
        public override void Dispose()
        {
            this.Ax.Accepted -= Ax_Accepted;
            this.Ax.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// 连接终端.
        /// </summary>
        public virtual IList<TcpClient> Connects { get; set; }

        public void BindEvents(ITcpServerEvents events)
        {
            this.events = events;
        }

        /// <summary>
        /// 监听.
        /// </summary>
        /// <param name="address">地址.</param>
        /// <param name="backlog">连接最大数量.</param>
        public virtual void Listen(string address)
        {
            this.Listen(new SocketUri(address));
        }

        /// <summary>
        /// 监听.
        /// </summary>
        /// <param name="address">地址.</param>
        /// <param name="backlog">连接最大数量.</param>
        public virtual void Listen(SocketUri address)
        {
            if (address != null)
            {
                this.Address = address;
                this.IPEndPoint = new IPEndPoint(address.IP, address.Port);
                this.Socket = new Socket(address.IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                this.TcpConfigure(this);
                this.Bind();
                this.Listen(Consts.MaxAcceptClient);
                if (address.Port == 0)
                {
                    // 端口由socket 分配
                    var ip = Socket.LocalEndPoint as IPEndPoint;
                    if (ip != null)
                    {
                        Address = new SocketUri($"{Address.Scheme}{SocketUri.SchemeDelimiter}{Address.Host}{SocketUri.PortDelimiter}{ip.Port}");
                        IPEndPoint.Port = ip.Port;
                    }
                }
                this.OnConnected();
            }
        }

        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="address">监听地址</param>
        /// <param name="closed">服务关闭回调</param>
        public virtual void Run(string address, TcpClosedFunction closed = null)
        {
            alive = true;
            this.closed_function = closed;
            Listen(address);
        }

        private void Ax_Accepted(Socket socket)
        {
            var client = new TcpClient(socket, this);
            client.WhenLoseConnected(this);
            lock (this.Connects) { this.Connects.Add(client); }
            OnAccepted(client);
        }

        /// <summary>
        /// 当丢失客户端
        /// </summary>
        /// <param name="losed">处理函数</param>
        public virtual void WhenClientLosed(TcpClientLosedFunction losed)
        {
            this.losed_function = losed;
        }

        /// <summary>
        /// 客户端丢失
        /// </summary>
        public virtual void OnSocketLosed(object socket)
        {
            var client = (TcpClient)socket;
            this.events?.OnSocketLosed(this, client);
            if (client != null)
            {
                lock (this.Connects) { this.Connects.Remove(client); }
                //clear_socket();
                //client.Dispose();
                if(this.losed_function != null) losed_function.Invoke(client);
            }
        }

        protected virtual void OnAccepted(object client)
        {
            accepted_function?.Invoke(this, client as TcpClient);
            this.events?.OnAccepted(this, client as TcpClient);
        }

        protected virtual void OnListened()
        {
            this.events?.OnListened(this);
            if (alive) Ax.StartAccept();
        }

        protected override void OnClosed()
        {
            alive = false;
            this.Connects.ToList().ForEach(con => con.Dispose());
            closed_function?.Invoke();
            base.OnClosed();
            this.events.OnClosed(this);
            this.Dispose();
        }

        protected override void OnConfiguration()
        {
            base.OnConfiguration();
            this.events.OnConfiguration(this);
        }
        private void clear_socket()
        {
            IList<TcpClient> loses;
            lock (this.Connects)
            {
                loses = this.Connects.Where(con => !con.Actived).ToList();
                foreach (var lose in loses)
                {
                    this.Connects.Remove(lose);
                }
            }
            foreach (var lose in loses) lose.Dispose();
        }
    }
}
