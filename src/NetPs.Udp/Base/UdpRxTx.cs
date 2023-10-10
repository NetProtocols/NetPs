namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Reactive.Linq;

    public class UdpRxTx<TTx, TRx> : UdpCore
        where TTx : IUdpTx, new()
        where TRx : IUdpRx, new()
    {
        private List<IUdpTx> txs = new List<IUdpTx>();
        public UdpRxTx()
        {
            this.Rx = new TRx();
            this.Rx.BindCore(this);
        }

        /// <summary>
        /// Gets or sets 接收.
        /// </summary>
        public IUdpRx Rx { get; protected set; }

        /// <summary>
        /// 开始接收数据
        /// </summary>
        public void StartReveice(Action<UdpData> action)
        {
            if (action != null) this.Disposables.Add(this.ReceicedObservable.Subscribe(action));
            this.Rx.StartReveice();
        }

        public void StartReveice()
        {
            this.Rx.StartReveice();
        }

        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<UdpData> ReceicedObservable => this.Rx.ReceicedObservable;

        public IUdpTx GetTx(IPEndPoint address)
        {
            var tx = this.txs.Find(t => t.RemoteIP.Equals(address));
            if (tx == null)
            {
                lock (txs)
                {
                    tx = new TTx();
                    tx.SetRemote(address);
                    tx.BindCore(this);
                    txs.Add(tx);
                }
                tx.AddDispose(tx.TransportedObservable.Subscribe(observer =>
                {
                    lock (txs)
                    {
                        this.txs.Remove(tx);
                    }
                }));
            }
            return tx;
        }
        public IUdpTx GetTx(IPAddress ip, int port)
        {
            return this.GetTx(new IPEndPoint(ip, port));
        }

        public IUdpTx GetTx(string address)
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
