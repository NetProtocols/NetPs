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
            this.Bind("0.0.0.0:0");
        }
    }
}
