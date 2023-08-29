namespace NetPs.Udp
{
    using NetPs.Udp.DNS;
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    public delegate void DNSReceivedHandler(DnsPacket packet);
    public class DnsHost : IDisposable
    {
        #region 常见DNS
        public const string DNS_NETEASE = "114.114.114.114";
        public const string DNS_ALI = "223.6.6.6";
        public const string DNS_ALI_2 = "223.5.5.5";
        public const string DNS_GOOGLE = "8.8.8.8";
        public const string DNS_GOOGLE_2 = "8.8.4.4";
        public const string DNS_TENCENT = "119.29.29.29";
        public const string DNS_TENCENT_2 = "182.254.116.116";
        public const string DNS_BAIDU = "180.76.76.76";
        public const string DNS_360 = "101.198.198.198";
        public const string DNS_CNNIC = "1.2.4.8";
        public const string DNS_CNNIC_2 = "210.2.4.8";
        public const string DNS_NORTONCONNECTSAFE = "199.85.126.10";
        public const string DNS_NORTONCONNECTSAFE_2 = "199.85.127.10";
        public const string DNS_OPENDNS = "208.67.222.222";
        public const string DNS_OPENDNS_2 = "208.67.220.220";
        public const string DNS_DNSWATCH = "84.200.69.80";
        public const string DNS_DNSWATCH_2 = "84.200.70.40";
        public const string DNS_COMODO = "8.26.56.26";
        public const string DNS_COMODO_2 = "8.20.247.20";
        public const string DNS_VERISIGN = "64.6.64.6";
        public const string DNS_VERISIGN_2 = "64.6.65.6";
        public const string DNS_OPENNIC = "192.95.54.3";
        public const string DNS_OPENNIC_2 = "192.95.54.1";
        public const string DNS_GREENTEAM = "81.218.119.11";
        public const string DNS_GREENTEAM_2 = "209.88.198.133";
        public const string DNS_CLOUDFLARE = "1.1.1.1";
        #endregion
        public const int DEFAUlT_TIMEOUT = 2000;
        public const int DEFAULT_RETRY_TIMES = 2;
        private static ushort id = 1;
        protected readonly CompositeDisposable disposables;
        private UdpHost host;
        public int TimeoutMillisenconds { get; }
        public int RetryTimes { get; }
        public DnsHost(string address, int timeout = DEFAUlT_TIMEOUT, int retry_times = DEFAULT_RETRY_TIMES) : this(timeout, retry_times)
        {
            host = new UdpHost(address);
        }

        public DnsHost(int timeout = DEFAUlT_TIMEOUT, int retry_times = DEFAULT_RETRY_TIMES)
        {
            this.TimeoutMillisenconds = timeout;
            this.RetryTimes = retry_times;
            if (host == null)
            {
                host = new UdpHost();
            }
            this.disposables = new CompositeDisposable();
            this.PacketReceivedObservable = Observable.FromEvent<DNSReceivedHandler, DnsPacket>(handler => packet => handler(packet), evt => this.PacketReceived += evt, evt => this.PacketReceived -= evt)
                    .Timeout(TimeSpan.FromMilliseconds(TimeoutMillisenconds));
            host.Rx.Received += Rx_Received;
            host.Rx.StartReveice();
        }

        public virtual event DNSReceivedHandler PacketReceived;

        public virtual IObservable<DnsPacket> PacketReceivedObservable { get; protected set; }
        public void Dispose()
        {
            this.disposables.Dispose();
            
            if (host != null)
            {
                host.Rx.Received -= Rx_Received;
                host.Dispose();
                host = null;
            }
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
            var rep = this.PacketReceivedObservable
                    .FirstAsync(_p => _p.TransactionID == req_packet.TransactionID);
            using (var tx = host.GetTx(address))
            {
                for (var i = RetryTimes; i!=0; i--)
                {
                    var task = rep.GetAwaiter();
                    tx.Transport(req_packet.GetData());
                    try
                    {
                        var packet = await task;
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

        private void Rx_Received(UdpData data)
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
