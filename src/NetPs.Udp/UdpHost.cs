namespace NetPs.Udp
{
    using System;
    using System.Net;

    public class UdpHost: UdpHostFactory<UdpTx, UdpRx>
    {
        public UdpHost(string address): base(address) {
        }
        public UdpHost(IPAddress ip, int port = 0): base(ip, port)
        {
        }
        public UdpHost() : base()
        {
        }
    }
}
