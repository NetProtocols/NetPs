namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;

    /// <summary>
    /// Tcp 收发.
    /// </summary>
    public class TcpRxTx<TTx, TRx> : TcpCore where TTx : TcpTx, ITx, new() where TRx: TcpRx, IRx, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpRxTx"/> class.
        /// </summary>
        /// <param name="tcp_config">配置.</param>
        public TcpRxTx(TcpConfigFunction tcp_config)
            : base(tcp_config)
        {
            this.Rx = new TRx();
            this.Tx = new TTx();
            this.Rx.BindCore(this);
            this.Tx.BindCore(this);
        }

        public TcpRxTx() : base()
        {
        }

        /// <summary>
        /// Gets or sets 接收.
        /// </summary>
        public TRx Rx { get; protected set; }

        /// <summary>
        /// Gets or sets 发送.
        /// </summary>
        public TTx Tx { get; protected set; }


        protected override void OnClosed()
        {
            base.OnClosed();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
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