﻿namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net.Sockets;

    /// <summary>
    /// Tcp 客户端工厂.
    /// </summary>
    public class TcpClientFactory<TTx, TRx> : TcpRxTx<TTx, TRx>, ITcpClient
        where TTx : ITcpTx, new() where TRx : ITcpRx, new()
    {
        private ITcpClientEvents events { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="tcp_config">Socket配置.</param>
        public TcpClientFactory(TcpConfigFunction tcp_config = null) : base(tcp_config)
        {
        }

        public TcpClientFactory(ITcpClientEvents events) : base()
        {
            this.events = events;
        }

        public TcpClientFactory(Socket socket) : base(socket)
        {
        }

        /// <summary>
        /// Gets or sets 发送完成.
        /// </summary>
        public virtual IObservable<TcpTx> TransportedObservable => this.Tx.TransportedObservable;

        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<byte[]> ReceivedObservable => this.Rx.ReceivedObservable;
        /// <summary>
        /// 开始接收数据
        /// </summary>
        public void StartReceive() => this.Rx.StartReceive();

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        public void Transport(byte[] data) => Tx.Transport(data);
        public void Transport(byte[] data, int offset, int length) => Tx.Transport(data, offset, length);

        /// <summary>
        /// 开始用指定接口接收数据
        /// </summary>
        public void StartReceive(ITcpReceive receive)
        {
            this.Disposables.Add(this.Rx.ReceivedObservable.Subscribe(data => receive.TcpReceive(data, this)));
            this.Rx.StartReceive();
        }
        public void BindEvents(ITcpClientEvents events)
        {
            this.events = events;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void OnClosed()
        {
            this.events?.OnClosed(this);
            base.OnClosed();
        }
        protected override void OnConfiguration()
        {
            base.OnConfiguration();
            this.Socket.Blocking = false;
            this.events?.OnConfiguration(this);
        }

        protected override void OnConnected()
        {
            this.events?.OnConnected(this);
            base.OnConnected();
        }
        protected override void OnDisconnected()
        {
            this.events?.OnDisconnected(this);
            base.OnDisconnected();
            this.Dispose();
        }
        protected override void OnLosed()
        {
            this.events?.OnLosed(this);
            base.OnLosed();
            this.OnDisconnected();
        }

        public void BindRxEvents(IRxEvents events)
        {
            this.Rx.BindEvents(events);
        }

        public void BindTxEvents(ITxEvents events)
        {
            this.Tx.BindEvents(events);
        }

        public ITx GetTx()
        {
            return this.Tx;
        }

        public IRx GetRx()
        {
            return this.Rx;
        }
    }
}
