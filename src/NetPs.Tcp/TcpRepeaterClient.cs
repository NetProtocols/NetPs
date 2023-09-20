namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;

    public class TcpRepeaterClient : TcpClientFactory<TcpTx, TcpRxRepeater>, IDisposable
    {
        private bool is_disposed = false;
        private TcpClient tcpClient { get; set; }
        internal TcpRepeaterClient()
        {
        }
        public TcpRepeaterClient(IDataTransport transport) : base()
        {
            this.Rx.BindTransport(transport);
        }

        public TcpRepeaterClient(TcpClient client, IDataTransport transport) : base()
        {
            this.tcpClient = client;
            this.PutSocket(client.Socket);
            this.Rx.BindTransport(transport);
        }
        public override bool IsDisposed => base.IsDisposed || this.is_disposed;
        public virtual void Limit(int limit) => this.Rx.SetLimit(limit);
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
