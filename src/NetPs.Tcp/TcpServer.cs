namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;

    /// <summary>
    /// .
    /// </summary>
    public class TcpServer : TcpCore, IDisposable, ISocketLose
    {
        private bool alive = false;

        public TcpServer(Action<TcpCore> tcp_config = null) : base(tcp_config)
        {
            construct();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        public TcpServer(Action<TcpServer, TcpClient> accepted, Action<TcpCore> tcp_config = null) : base(tcp_config)
        {
            construct();
            this.Disposables.Add(this.AcceptObservable.Subscribe(s =>
            {
                var client = new TcpClient(s, this);
                lock (this.Connects) { this.Connects.Add(client); }
                accepted?.Invoke(this, client);
            }));
        }

        public TcpServer(ITcpServerConfig serverConfig) : base(serverConfig)
        {
            construct();
            this.Disposables.Add(this.AcceptObservable.Subscribe(s =>
            {
                var client = new TcpClient(s, this);
                if (serverConfig.TcpAccept(this, client))
                {
                    if (client.Actived)
                    {
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
            this.Ax?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// 连接终端.
        /// </summary>
        public IList<TcpClient> Connects { get; set; }

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
                this.Tcp_config?.Invoke(this);
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

        private Action x_closed { get; set; }
        public void Run(string address, Action closed = null)
        {
            alive = true;
            this.x_closed = closed;
            Listen(address);
        }

        /// <summary>
        /// 客户端丢失
        /// </summary>
        public void SocketLosed(object socket)
        {
            var client = (TcpClient)socket;
            if (client != null)
            {
                lock (this.Connects) { this.Connects.Remove(client); }
                //client.Dispose();
            }
        }
        protected override void OnConnected()
        {
            base.OnConnected();
            if (alive) Ax.StartAccept();
        }

        protected override void OnClosed()
        {
            alive = false;
            this.Connects.ToList().ForEach(con => con.Dispose());
            x_closed?.Invoke();
            base.OnClosed();
            this.Dispose();
        }
    }
}
