namespace NetPs.Udp
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class UdpHost : UdpRxTx
    {
        public UdpHost(string address): base() {
            this.Bind(address);
        }
        public UdpHost() : base()
        {
            this.Bind("0.0.0.0:0");
        }
    }
}
