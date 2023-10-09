namespace NetPs.Udp.Wol
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using NetPs.Socket;
    using NetPs.Udp;

    public class WolSender : IDisposable
    {
        private IList<UdpHost> hosts { get; set; }
        public WolSender()
        {
            hosts = new List<UdpHost>();
            var ips = new HostIPList().IPv4s();
            foreach (var ip in ips)
            {
                var host = new UdpHost(ip);
                //不需要接收
                //host.StartReveice();
                hosts.Add(host);
            }
        }
        public void Send(string mac)
        {
            var pkt = new WakeOnLanPacket(mac);
            foreach (var host in hosts)
            {
                Task.Factory.StartNew(() =>
                {
                    var tx = host.GetTx(host.Address.ToBroadcast(), 9);
                    tx.Transport(pkt.GetData());
                    Thread.Sleep(200);
                    tx.Transport(pkt.GetData());
                    Thread.Sleep(200);
                    tx.Transport(pkt.GetData());
                });
            }
        }

        public void Dispose()
        {
            foreach (var host in hosts) host.Dispose();
            this.hosts.Clear();
        }
    }
}
