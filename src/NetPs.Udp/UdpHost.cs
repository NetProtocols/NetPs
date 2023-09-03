namespace NetPs.Udp
{
    using System;

    public class UdpHost : UdpRxTx
    {
        public UdpHost(string address): base() {
            this.Bind(address);
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
