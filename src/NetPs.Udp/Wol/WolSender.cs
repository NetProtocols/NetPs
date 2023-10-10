namespace NetPs.Udp.Wol
{
    using System;
    using System.Collections.Generic;
    using NetPs.Socket;
    using NetPs.Udp;

    public class WolSender : IDisposable
    {
        private IList<IUdpHost> hosts { get; set; }
        /// <summary>
        /// WakeOnLan 网络唤醒
        /// </summary>
        public WolSender()
        {
            this.MAC_Length = 6;
            this.ReTimes = 3;
            hosts = new List<IUdpHost>();
            var ips = new HostIPList().IPv4s();
            foreach (var ip in ips)
            {
                var host = new UdpHostFactory<UdpQueueTx, UdpRx>(ip);
                //不需要接收
                //host.StartReveice();
                hosts.Add(host);
            }
        }
        public int MAC_Length { get; private set; }
        public int ReTimes { get; private set; }
        public void Send(string mac)
        {
            var pkt = new WakeOnLanPacket(mac);
            foreach (var host in hosts)
            {
                var tx = host.GetTx(host.Address.ToBroadcast(), 9);
                var pkt_data = pkt.GetData();
                tx.SetTransportBufferSize(pkt_data.Length);
                for(var i=0; i<ReTimes; i++) tx.Transport(pkt_data);
            }
        }
        public void SetMacLength(int length)
        {
            if (length < 6) return;
            this.MAC_Length = length;
        }
        public void SetReTimes(int times)
        {
            if (times < 1) return;
            this.ReTimes = times;
        }
        public void Dispose()
        {
            foreach (var host in hosts) host.Dispose();
            this.hosts.Clear();
        }
    }
}
