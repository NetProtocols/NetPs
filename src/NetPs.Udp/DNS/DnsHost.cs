using NetPs.Udp.DNS;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPs.Udp
{
    public delegate void DNSReceivedHandler(DnsPacket packet);
    public class DnsHost : IDisposable
    {
        private static ushort id = 1;
        protected readonly CompositeDisposable disposables;
        private UdpHost host;
        public DnsHost(string address) : this()
        {
            host = new UdpHost(address);
        }

        public DnsHost()
        {
            if (host == null)
            {
                host = new UdpHost();
            }
            host.Rx.Reveiced += Rx_Reveiced;
            this.disposables = new CompositeDisposable();
            this.PacketReceivedObservable = Observable.FromEvent<DNSReceivedHandler, DnsPacket>(handler => packet => handler(packet), evt => this.PacketReceived += evt, evt => this.PacketReceived -= evt);
            host.Rx.StartReveice();
        }

        public virtual event DNSReceivedHandler PacketReceived;

        public virtual IObservable<DnsPacket> PacketReceivedObservable { get; protected set; }
        public void Dispose()
        {
            this.disposables?.Dispose();
            if (host != null) { host.Dispose(); }
        }

        public void Send(DnsPacket packet)
        {
            using (var tx = host.GetTx(packet.IPEndPoint))
            {
                tx.Transport(packet.GetData());
            }
        }
        public async Task<DnsPacket> SendReq(string address, DnsPacket req_packet)
        {
            using (var tx = host.GetTx(address))
            {
                for (var i = 3; i!=0; i--)
                {
                    tx.Transport(req_packet.GetData());
                    try
                    {
                        var packet = await this.PacketReceivedObservable
                            .Timeout(TimeSpan.FromSeconds(2))
                            .FirstAsync(_p => _p.TransactionID == req_packet.TransactionID);
                        return packet;
                    }
                    catch (TimeoutException) { continue; }
                }
            }
            throw new TimeoutException(address);
        }

        public async Task<DnsPacket> SendReq(string address, ushort type, string name)
        {
            return await SendReq(address, new DnsPacket
            {
                TransactionID = id++,
                Queries = new[]
                {
                    new DnsQuestion
                    {
                        Class = DnsPacket.Class_IN,
                        Type = type,
                        Name = name
                    }
                }
            });
        }

        public async Task<DnsPacket> SendReqA(string address, string name)
        {
            return await SendReq(address, DnsPacket.Type_A, name);
        }
        public async Task<DnsPacket> SendReqAAAA(string address, string name)
        {
            return await SendReq(address, DnsPacket.Type_AAAA, name);
        }
        public async Task<DnsPacket> SendReqCNAME(string address, string name)
        {
            return await SendReq(address, DnsPacket.Type_CNAME, name);
        }

        private void Rx_Reveiced(UdpData data)
        {
            var packet = new DnsPacket(data.Data);
            packet.IPEndPoint = data.IP;
            if (PacketReceived != null) 
            {
                PacketReceived.Invoke(packet);
            }
        }
    }
}
