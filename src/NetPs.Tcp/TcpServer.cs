namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;

    /// <summary>
    /// .
    /// </summary>
    public class TcpServer : TcpCore, IDisposable
    {
        private object locker = new object();
        private bool alive = false;
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpServer"/> class.
        /// </summary>
        public TcpServer(Action<TcpServer, TcpClient> accepted, Action<TcpCore> tcp_config = null) : base(tcp_config)
        {
            this.Ax = new TcpAx(this);

            this.Connects = new List<TcpClient>();
            this.Disposables.Add(this.AcceptObservable.Subscribe(s =>
            {
                var client = new TcpClient();
                client.PutSocket(s);
                client.Disposables.Add(client.LoseConnectedObservable.Subscribe(_ =>
                {
                    lock (this.locker)
                    {
                        this.Connects.Remove(client);
                    }
                    client.Dispose();
                }));
                if (client.Actived)
                {
                    lock (this.locker)
                    {
                        this.Connects.Add(client);
                    }
                }

                accepted?.Invoke(this, client);
            }));
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
                this.OnConnected();
            }
        }

        public void Run(string address, Action closed = null)
        {
            alive = true;
            Disposables.Add(ConnectedObservable.Subscribe(_ =>
            {
                Ax.StartAccept();
            }));
            Disposables.Add(LoseConnectedObservable.Subscribe(_ =>
            {
                alive = false;
                Dispose();
                closed?.Invoke();
            }));
            Listen(address);
        }
    }
}
