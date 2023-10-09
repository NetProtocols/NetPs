namespace NetPs.Socket
{
    using NetPs.Socket.Operations;
    using System;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// 本机IP 地址列表.
    /// </summary>
    public class HostIPList
    {
        private IPAddress[] ips;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostIPList"/> class.
        /// </summary>
        public HostIPList()
        {
            this.Load();
        }

        /// <summary>
        /// Gets iP地址列表.
        /// </summary>
        public IPAddress[] IPs => this.ips;

        /// <summary>
        /// 加载.
        /// </summary>
        public virtual void Load()
        {
#if NET35_CF
            this.ips = ArrayTool.FindAll(Dns.GetHostEntry(Dns.GetHostName()).AddressList, f => !IPAddress.IsLoopback(f));
#else
            //需要筛选状态为运行的interface
            var interfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                .Where(f => f.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up && f.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback);
            var unicasts = interfaces.SelectMany(f => f.GetIPProperties().UnicastAddresses);
            this.ips = unicasts
                .Where(f => !IPAddress.IsLoopback(f.Address))
                .Select(f => new IPAddressWithMask(f.Address.GetAddressBytes(), f.IPv4Mask.GetAddressBytes()))
                .ToArray();
            //this.ips = ArrayTool.FindAll(Dns.GetHostAddresses(Dns.GetHostName()), f => !IPAddress.IsLoopback(f));

#endif
        }

        /// <summary>
        /// 是否存在IPV4.
        /// </summary>
        /// <returns>状态.</returns>
        public virtual bool ExistsIpv4()
        {
            return ArrayTool.Exist(this.IPs, f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }

        /// <summary>
        /// 是否存在IPV6.
        /// </summary>
        /// <returns>状态.</returns>
        public virtual bool ExistsIpv6()
        {
            return ArrayTool.Exist(this.IPs, f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
        }

        /// <summary>
        /// 获取IPv4 清单.
        /// </summary>
        /// <returns>IPv4 清单.</returns>
        public virtual IPAddress[] IPv4s()
        {
            return ArrayTool.FindAll(this.IPs, f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }

        /// <summary>
        /// 获取IPv6清单.
        /// </summary>
        /// <returns>IPv6清单.</returns>
        public virtual IPAddress[] IPv6s()
        {
            return ArrayTool.FindAll(this.IPs, f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
        }

        /// <summary>
        /// 获取IP.
        /// </summary>
        /// <returns>IP地址.</returns>
        public virtual IPAddress GetIP()
        {
            if (this.ExistsIpv4())
            {
                return this.IPv4s()[0];
            }

            if (this.ExistsIpv6())
            {
                return this.IPv6s()[0];
            }

            throw new Exception("CannotGetIP");
        }
    }
}
