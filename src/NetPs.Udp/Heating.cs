namespace NetPs.Socket
{
    using NetPs.Socket.Eggs;
    using NetPs.Udp;
    using NetPs.Udp.Base;
    using NetPs.Udp.DNS;
    using NetPs.Udp.Wol;
    using System;
    using System.Net;

    /// <summary>
    /// 加热类
    /// </summary>
    public sealed class Heat_udp : IHeat
    {
        private readonly byte[] test_data = { 0 } ;
        public void Start(IHeatingWatch watch)
        {
            new UdpHost().Dispose();
            new UdpRxRepeater().Dispose();
            new UdpTx().Dispose();
            new UdpQueueTx().Dispose();
            new UdpRx().Dispose();
            watch.Heat_Progress();
            Punycode.Decode("test.com");
            new DnsHost().Dispose();
            watch.Heat_Progress();
            new WolSender().Dispose();
        }
    }
}
