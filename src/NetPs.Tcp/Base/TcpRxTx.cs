namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;

    /// <summary>
    /// Tcp 收发.
    /// </summary>
    public class TcpRxTx<TTx, TRx> : TcpCore where TTx : ITcpTx, new() where TRx : ITcpRx, new()
    {
        private bool is_disposed = false;
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpRxTx"/> class.
        /// </summary>
        /// <param name="tcp_config">配置.</param>
        public TcpRxTx(TcpConfigFunction tcp_config)
            : base(tcp_config)
        {
            construct();
        }

        public TcpRxTx(System.Net.Sockets.Socket socket) : base(socket)
        {
            construct();
        }
        public TcpRxTx() : base()
        {
            construct();
        }

        private void construct()
        {
            this.Rx = this._Rx = new TRx();
            this.Tx = this._Tx = new TTx();
            this._Rx.BindCore(this);
            this._Tx.BindCore(this);
        }
        public override bool IsDisposed => base.IsDisposed || this.is_disposed;

        /// <summary>
        /// Gets or sets 接收.
        /// </summary>
        public virtual ITcpRx Rx { get; protected set; }

        /// <summary>
        /// Gets or sets 发送.
        /// </summary>
        public virtual ITcpTx Tx { get; protected set; }
        protected virtual TTx _Tx { get; private set; }
        protected virtual TRx _Rx { get; private set; }


        protected override void OnClosed()
        {
            base.OnClosed();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            if (this.Rx != null)
            {
                this.Rx.Dispose();
                this.Rx = null;
            }
            if (this.Tx != null)
            {
                this.Tx.Dispose();
                this.Tx = null;
            }
            base.Dispose();
        }
    }
}