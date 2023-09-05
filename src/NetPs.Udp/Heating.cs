namespace NetPs.Socket
{
    using NetPs.Socket.Eggs;
    using NetPs.Udp;
    using NetPs.Udp.DNS;
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
            //var udp = new UdpHost();
            //var dns = new DnsHost(1, 1);
            //udp.StartReveice();
            //watch.Heat_Progress();
            //await dns.SendReqA(IPAddress.Loopback.ToString(), "test.cn");
            //watch.Heat_Progress();
            //using (var tx = udp.GetTx(udp.IPEndPoint))
            //{
            //    tx.Transport(test_data);
            //}
            //Punycode.Decode("test.com");
            //var static_val = Consts.ReceiveBytes;
            //static_val = Consts.SocketPollTime;
            //static_val = Consts.TransportBytes;
            //watch.Heat_Progress();
            //udp.Dispose();
            //dns.Dispose();
        }
    }
}
