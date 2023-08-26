namespace NetPs.Tcp
{
    using System;

    /// <summary>
    /// Tcp 收发.
    /// </summary>
    public class TcpRxTx : TcpCore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpRxTx"/> class.
        /// </summary>
        /// <param name="tcp_config">配置.</param>
        public TcpRxTx(Action<TcpCore> tcp_config = null)
            : base(tcp_config)
        {
            this.Rx = new TcpRx(this);
            this.Tx = new TcpTx(this);
        }

        /// <summary>
        /// Gets or sets 接收.
        /// </summary>
        public TcpRx Rx { get; protected set; }

        /// <summary>
        /// Gets or sets 发送.
        /// </summary>
        public TcpTx Tx { get; protected set; }

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
            this.Rx?.Dispose();
            this.Tx?.Dispose();
        }
    }
}