namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;

    public class TcpRepeaterClient : TcpClientFactory<TcpTx, TcpRxRepeater>, IDisposable
    {
        private bool is_disposed = false;
        private TcpClient tcpClient { get; set; }
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

        public void Limit(int limit) => this.Rx.SetLimit(limit);

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
