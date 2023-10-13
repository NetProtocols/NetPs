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
        private bool is_disposed = false;
        private List<IUdpTx> txs = new List<IUdpTx>();
        public UdpRxTx()
        {
            this.Rx = new TRx();
            this.Rx.BindCore(this);
        }
        public UdpRxTx(IUdpHost host) : this()
        {
            this.PutSocket(host.Socket);
            if (host.GetRx() is IUdpRx udp_rx)
            {
                this.Rx.UseRx(udp_rx);
            }
        }

        /// <summary>
        /// 接收.
        /// </summary>
        public IUdpRx Rx { get; protected set; }
        /// <summary>
        /// 发送
        /// </summary>
        public IUdpTx Tx { get; protected set; }
        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<UdpData> ReceivedObservable => this.Rx.ReceivedObservable;

        /// <summary>
        /// 开始接收数据
        /// </summary>
        public void StartReceive(Action<UdpData> action)
        {
            if (action != null) this.Disposables.Add(this.ReceivedObservable.Subscribe(action));
            this.Rx.StartReceive();
        }
        public void StartReceive()
        {
            this.Rx.StartReceive();
        }
        public void Connect(IPEndPoint address)
        {
            this.RemoteIPEndPoint = address;
            this.RemoteAddress = new InsideSocketUri(InsideSocketUri.UriSchemeUDP, address);
            this.Rx.SetRemoteAddress(address);
            this.Tx = this.GetTx(address);
        }
        public void Connect(IPAddress ip, int port)
        {
            Connect(new IPEndPoint(ip, port));
        }
        public void Connect(string address)
        {
            var uri = new InsideSocketUri(address);
            this.Connect(uri.IP, uri.Port);
            this.IsUdp();
        }
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
                tx.WhenDisposed(_tx =>
                {
                    lock (txs)
                    {
                        this.txs.Remove(_tx);
                    }
                });
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
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
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
