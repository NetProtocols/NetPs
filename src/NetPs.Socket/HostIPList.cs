namespace NetPs.Socket
{
    using NetPs.Socket.Operations;
    using System;
    using System.Collections.Generic;
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
            int i, j;
            System.Net.NetworkInformation.NetworkInterface[] ins;
            Queue<IPAddress> ips;
            System.Net.NetworkInformation.UnicastIPAddressInformationCollection collet;

            ins = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            ips = new Queue<IPAddress>(ins.Length);
            for (i = ins.Length -1; i >= 0; i--)
            {
                if (ins[i].OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up || ins[i].NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback) continue;
                collet = ins[i].GetIPProperties().UnicastAddresses;
                for (j = collet.Count -1; j >= 0; j--)
                {
                    if (IPAddress.IsLoopback(collet[j].Address)) continue;
                    ips.Enqueue(new IPAddressWithMask(collet[j].Address.GetAddressBytes(), collet[j].IPv4Mask.GetAddressBytes()));
                }
            }

            this.ips = ips.ToArray();
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
