using NetPs.Tcp.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetPs.Tcp
{
    public class TcpRepeaterClient : TcpRxTx, IDisposable
    {
        private TcpRxRepeater x_rx { get; set; }
        public TcpRepeaterClient(IDataTransport transport) : base()
        {
            this.Rx = x_rx = new TcpRxRepeater(this, transport);
            this.Tx = new TcpTx(this);
        }

        public TcpRepeaterClient(TcpClient client, IDataTransport transport) : base()
        {
            this.PutSocket(client.Socket);
            this.Rx = x_rx = new TcpRxRepeater(this, transport);
        }

        public void Limit(int limit) => this.x_rx.SetLimit(limit);

        void IDisposable.Dispose()
        {
            if (this.Rx != null) this.Rx.Dispose();
            if (this.Tx != null) this.Tx.Dispose();
            this.Rx = null;
            base.Dispose();
        }
    }
}
