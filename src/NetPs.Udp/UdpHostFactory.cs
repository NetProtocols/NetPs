namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Net;
    //<UdpTx, UdpRx>
    public class UdpHostFactory<TTx, TRx> : UdpRxTx<TTx, TRx>, IUdpHost
        where TTx : IUdpTx, new()
        where TRx : IUdpRx, new()
    {
        public UdpHostFactory(string address): base() {
            this.Bind(address);
        }
        public UdpHostFactory(IPAddress ip, int port = 0): base()
        {
            this.Bind(new InsideSocketUri(InsideSocketUri.UriSchemeUDP, ip, port));
        }
        public UdpHostFactory() : base()
        {
        }
        public UdpHostFactory(IUdpHost host) : base(host)
        {
            this.ChangeAddress(host.Address);
        }
        public IUdpHost Clone(IPEndPoint address)
        {
            var host = new UdpHostFactory<TTx, TRx>(this);
            host.Connect(address);
            return host;
        }
        public ITx GetTx()
        {
            return this.Tx;
        }
        public IRx GetRx()
        {
            return this.Rx;
        }
    }
}
