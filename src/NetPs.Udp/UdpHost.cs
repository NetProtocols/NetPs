namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Net;

    public class UdpHost : UdpRxTx
    {
        public UdpHost(string address): base() {
            this.Bind(address);
        }
        public UdpHost(IPAddress ip, int port = 0): base()
        {
            this.Bind(new InsideSocketUri(InsideSocketUri.UriSchemeUDP, ip, port));
        }
        public UdpHost() : base()
        {
            this.Bind("0.0.0.0:0");
        }

        protected override void OnClosed()
        {
            base.OnClosed();
        }
    }
}
