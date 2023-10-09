namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;

    public class TcpRepeaterClient : TcpClientFactory<TcpTx, TcpRxRepeater>, IDisposable
    {
        private bool is_disposed = false;
        private ITcpClient tcpClient { get; set; }
        internal TcpRepeaterClient()
        {
        }
        public TcpRepeaterClient(IDataTransport transport) : base()
        {
            this._Rx.BindTransport(transport);
        }

        public TcpRepeaterClient(ITcpClient client, IDataTransport transport) : base(client.Socket)
        {
            this.tcpClient = client;
            this._Rx.BindTransport(transport);
        }
        public override bool IsDisposed => base.IsDisposed || this.is_disposed;
        public virtual void Limit(int limit) => this._Rx.SetLimit(limit);
        protected override void OnConfiguration()
        {
            base.OnConfiguration();
        }
        public override void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            if (this.tcpClient != null)
            {
                this.tcpClient.Lose();
                this.tcpClient = null;
            }
            base.Dispose();
        }
    }
}
