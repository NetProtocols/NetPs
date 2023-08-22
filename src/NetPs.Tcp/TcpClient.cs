﻿namespace NetPs.Tcp
{
    using System;
    using System.Net.Sockets;

    /// <summary>
    /// Tcp 客户端.
    /// </summary>
    public class TcpClient : TcpRxTx
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="tcp_config">Socket配置.</param>
        public TcpClient(Action<TcpCore> tcp_config = null): base(tcp_config)
        {
        }

        public TcpClient(Socket socket) : base(null)
        {
            PutSocket(socket);
        }

        /// <summary>
        /// Gets or sets 发送完成.
        /// </summary>
        public virtual IObservable<TcpTx> TransportedObservable => this.Tx.TransportedObservable;

        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<byte[]> ReceivedObservable => this.Rx.ReceivedObservable;

        public void StartReceive() => this.Rx.StartReceive();

        public void Transport(byte[] data) => Tx.Transport(data);

        public void StartReceive(ITcpReceive receive)
        {
            this.Disposables.Add(this.Rx.ReceivedObservable.Subscribe(data => receive.TcpReceive(data, this)));
            this.Rx.StartReceive();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
        }

        public IHub Hub { get; set; }
        /// <summary>
        /// 镜像模式.
        /// </summary>
        /// <param name="address">镜像来源.</param>
        public void StartMirror(string address)
        {
            if (Hub != null) Hub.Close();
            Hub = new MirrorHub(this, address);
            Hub.Start();
        }
    }
}