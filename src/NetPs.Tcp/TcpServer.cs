namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Reactive.Linq;

    public delegate void TcpAcceptedFunction(TcpServer tcpServer, ITcpClient tcpClient);
    public delegate void TcpClosedFunction();
    public delegate void TcpClientLosedFunction(ITcpClient tcpClient);
    /// <summary>
    /// Tcp server.
    /// </summary>
    public class TcpServer : TcpCore, IDisposable, ISocketLosed, ITcpServer
    {
        private bool alive = false;
        private bool is_disposed = false;
        private TcpAcceptedFunction accepted_function { get; set; }
        private TcpClosedFunction closed_function { get; set; }
        private TcpClientLosedFunction losed_function { get; set; }
        private ITcpServerEvents events { get; set; }
        public TcpServer(TcpConfigFunction tcp_config = null) : base(tcp_config)
        {
            construct();
            this.Ax.Accepted += Ax_Accepted;
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
                var client = new TcpClient(s);
                if (serverConfig.TcpAccept(this, client))
                {
                    if (client.Actived)
                    {
                        client.WhenLoseConnected(this);
                        client.Rx.WhenReceived(serverConfig);
                        add_connect(client);
                        return;
                    }
                }
                //无效客户端
                client.Dispose();
            }));
            this.Run(serverConfig.BandAddress);
        }

        private void construct()
        {
            this.Ax = new TcpAx();
            this.Ax.BindCore(this);
            this.Connects = new List<ITcpClient>();
            this.AcceptClientObservable = Observable.FromEvent<TcpAcceptedFunction, ITcpClient>(handler => (s, c) => handler(c), evt => this.AcceptedClient += evt, evt => this.AcceptedClient -= evt);
        }

        /// <summary>
        /// 运行状态
        /// </summary>
        public virtual bool Alive => this.alive;
        public override bool IsServer => true;
        public override bool IsDisposed => base.IsDisposed|| this.is_disposed;
        /// <summary>
        /// Gets or sets 接受.
        /// </summary>
        public virtual IObservable<Socket> AcceptObservable => this.Ax.AcceptObservable;

        public event TcpAcceptedFunction AcceptedClient;
        public virtual IObservable<ITcpClient> AcceptClientObservable { get; private set; }

        /// <summary>
        /// Gets or sets 服务.
        /// </summary>
        public virtual TcpAx Ax { get; protected set; }

        /// <inheritdoc/>
        public override void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            this.Ax.Accepted -= Ax_Accepted;
            this.Ax.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// 连接终端.
        /// </summary>
        public virtual IList<ITcpClient> Connects { get; set; }
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
            this.Listen(new InsideSocketUri(InsideSocketUri.UriSchemeTCP, address));
        }

        /// <summary>
        /// 监听.
        /// </summary>
        /// <param name="address">地址.</param>
        /// <param name="backlog">连接最大数量.</param>
        public virtual void Listen(ISocketUri address)
        {
            if (address != null)
            {
                this.Bind(address);
                this.Listen(Consts.MaxAcceptClient);
                this.OnListened();
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
            if (! socket.Connected) return;
            var client = new TcpClient(socket);
            add_connect(client);
            client.WhenLoseConnected(this);
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
            if (socket == null) return;
            if (socket is ITcpClient client)
            {
                remove_connect(client);
                this.events?.OnSocketLosed(this, client);
                if (this.losed_function != null) losed_function.Invoke(client);
            }
        }

        protected virtual void OnAccepted(object client)
        {
            if (client == null) return;
            if (client is ITcpClient tcpclient)
            {
                accepted_function?.Invoke(this, tcpclient);
                this.events?.OnAccepted(this, tcpclient);
                this.AcceptedClient?.Invoke(this, tcpclient);
            }
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
            this.events?.OnClosed(this);
            base.OnClosed();
        }
        protected override void OnLosed()
        {
            base.OnLosed();
            this.Dispose();
        }
        protected override void OnConfiguration()
        {
            base.OnConfiguration();
            this.events?.OnConfiguration(this);
        }

        private void add_connect(ITcpClient client)
        {
            lock (this.Connects) { this.Connects.Add(client); }
        }
        private void remove_connect(ITcpClient client)
        {
            lock (this.Connects) { this.Connects.Remove(client); }
        }
    }
}
