namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;

    public class TcpRepeaterClient : TcpRxTx, IDisposable
    {
        private TcpRxRepeater x_rx { get; set; }
        private TcpClient tcpClient { get; set; }
        public TcpRepeaterClient(IDataTransport transport) : base()
        {
            this.Rx = x_rx = new TcpRxRepeater(this, transport);
            this.Tx = new TcpTx(this);
        }

        public TcpRepeaterClient(TcpClient client, IDataTransport transport) : base()
        {
            this.tcpClient = client;
            this.PutSocket(client.Socket);
            this.Rx = x_rx = new TcpRxRepeater(this, transport);
        }

        public void Limit(int limit) => this.x_rx.SetLimit(limit);

        void IDisposable.Dispose()
        {
            if (this.tcpClient != null)
            {
                this.tcpClient.Dispose();
                this.tcpClient = null;
            }
            if (x_rx != null)
            {
                x_rx.Dispose();
                x_rx = null;
            }
            if (this.Tx != null) this.Tx.Dispose();
            base.Dispose();
        }
    }
}
