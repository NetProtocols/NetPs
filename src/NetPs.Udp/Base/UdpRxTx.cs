namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Reactive.Linq;

    public class UdpRxTx : UdpCore
    {
        private List<UdpTx> txs = new List<UdpTx>();
        public UdpRxTx()
        {
            this.Rx = new UdpRx(this);
        }

        /// <summary>
        /// Gets or sets 接收.
        /// </summary>
        public UdpRx Rx { get; protected set; }

        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<UdpData> ReceicedObservable => this.Rx.ReceicedObservable;

        public UdpTx GetTx(IPEndPoint address)
        {
            var tx = this.txs.Find(t => t.RemoteIP == address);
            if (tx == null)
            {
                lock (txs)
                {
                    tx = new UdpTx(this, address);
                    txs.Add(tx);
                }
                tx.Disposables.Add(tx.TransportedObservable.Subscribe(observer =>
                {
                    lock (txs)
                    {
                        this.txs.Remove(tx);
                    }
                }));
            }
            return tx;
        }

        public UdpTx GetTx(string address)
        {
            var uri = new SocketUri(address);
            return this.GetTx(new IPEndPoint(uri.IP, uri.Port));
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            this.Rx?.Dispose();
            foreach (var tx in this.txs) tx.Dispose();
            base.Dispose();
        }
    }
}
