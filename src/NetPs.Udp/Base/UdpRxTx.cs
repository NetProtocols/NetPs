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
            this.Rx = new UdpRx();
            this.Rx.BindCore(this);
        }

        /// <summary>
        /// Gets or sets 接收.
        /// </summary>
        public UdpRx Rx { get; protected set; }

        /// <summary>
        /// 开始接收数据
        /// </summary>
        public void StartReveice(Action<UdpData> action = null)
        {
            if (action == null) this.Disposables.Add(this.ReceicedObservable.Subscribe());
            else this.Disposables.Add(this.ReceicedObservable.Subscribe(action));
            this.Rx.StartReveice();
        }

        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<UdpData> ReceicedObservable => this.Rx.ReceicedObservable;

        public UdpTx GetTx(IPEndPoint address)
        {
            var tx = this.txs.Find(t => t.RemoteIP.Equals(address));
            if (tx == null)
            {
                lock (txs)
                {
                    tx = new UdpTx(address);
                    tx.BindCore(this);
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
        public UdpTx GetTx(IPAddress ip, int port)
        {
            return this.GetTx(new IPEndPoint(ip, port));
        }

        public UdpTx GetTx(string address)
        {
            var uri = new InsideSocketUri(InsideSocketUri.UriSchemeUDP, address);
            return this.GetTx(new IPEndPoint(uri.IP, uri.Port));
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            this.Rx?.Dispose();
            lock (txs)
            {
                txs.ForEach(tx => tx.Dispose());
            }
            base.Dispose();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            this.Dispose();
        }
    }
}
