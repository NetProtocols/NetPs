namespace NetPs.Socket
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;

    public interface ISocketUri
    {
        /// <summary>
        /// 协议
        /// </summary>
        string Scheme { get; }
        /// <summary>
        /// 网络地址
        /// </summary>
        IPAddress IP { get; }
        /// <summary>
        /// 主机名
        /// </summary>
        string Host { get; }
        /// <summary>
        /// 端口
        /// </summary>
        int Port { get; }
        string ToString();
    }

    public static class ISocketUriExtra
    {
        public static bool IsIpv6(this IPAddress ip)
        {
            return ip.AddressFamily is AddressFamily.InterNetworkV6;
        }
        public static bool IsIpv4(this IPAddress ip)
        {
            return ip.AddressFamily is AddressFamily.InterNetwork;
        }
        public static bool IsBroadcast(this IPAddress ip)
        {
            if (ip is IPAddressWithMask ip_withmask)
            {
                return ip_withmask.IsBroadcast();
            }
            return ip.GetAddressBytes().Last() == 0xff;
        }
        public static ISocketUri ResetPort(this ISocketUri uri, int port)
        {
            if (uri is InsideSocketUri inside)
            {
                inside.SetPort(port);
                return inside;
            }
            return new InsideSocketUri(uri, port);
        }
        public static bool IsIpv6(this ISocketUri uri)
        {
            return uri.IP.AddressFamily is AddressFamily.InterNetworkV6;
        }
        public static bool IsIpv4(this ISocketUri uri)
        {
            return uri.IP.AddressFamily is AddressFamily.InterNetwork;
        }
        public static bool IsAny(this ISocketUri uri)
        {
            return uri.IP == IPAddress.Any || uri.IP == IPAddress.IPv6Any;
        }
        public static bool IsTcp(this ISocketUri uri)
        {
            return uri.Scheme == InsideSocketUri.UriSchemeTCP;
        }
        public static bool IsUdp(this ISocketUri uri)
        {
            return uri.Scheme == InsideSocketUri.UriSchemeUDP;
        }
        public static ISocketUri ToLoopback(this ISocketUri uri)
        {
            if (uri.IsIpv4()) return new InsideSocketUri(uri.Scheme, IPAddress.Loopback, uri.Port);
            return  new InsideSocketUri(uri.Scheme, IPAddress.IPv6Loopback, uri.Port);
        }
        public static bool Equal(this ISocketUri uri, IPEndPoint ip)
        {
            if (ip == null) return false;
            return ip.Address.Equals(uri.IP) && (ip.Port == uri.Port || uri.Port == 0);
        }
        public static bool Equal(this ISocketUri uri, ISocketUri host)
        {
            return uri.IP.Equals(host.IP) && (uri.Port == host.Port || uri.Port == 0 || host.Port == 0);
        }
        public static IPAddress ToBroadcast(this ISocketUri uri)
        {
            //mask 不足以0补全，超出则放弃
            var bytes = uri.IP.GetAddressBytes();
            if (uri.IP is IPAddressWithMask maskip)
            {
                var ip_mask = maskip.NetMask;
                if (uri.CheckNetMask(ip_mask))
                {
                    for (var i = 0; i < ip_mask.Length; i++) bytes[i] |= ip_mask[i];
                    return new IPAddressWithMask(bytes, IPAddressWithMask.ReverseMask(ip_mask));
                }
            }
            return IPAddress.Broadcast;
        }
        public static IPAddress ToBroadcast(this ISocketUri uri, params byte[] netmask)
        {
            var broadcast = uri.ToBroadcast();
            if (broadcast.Equals(IPAddress.Broadcast))
            {
                //mask 不足以0补全，超出则放弃
                var bytes = uri.IP.GetAddressBytes();
                var ip_mask = new byte[bytes.Length];
                var j = 0;
                var i = 0;
                for (; i < bytes.Length; i++)
                {
                    if (i + netmask.Length < bytes.Length) ip_mask[i] = 0;
                    else ip_mask[i] = netmask[j++];
                }
                if (!uri.CheckNetMask(ip_mask)) return IPAddress.Broadcast;
                for (i = 0; i < ip_mask.Length; i++) bytes[i] |= ip_mask[i];
                return new IPAddressWithMask(bytes, IPAddressWithMask.ReverseMask(ip_mask));
            }
            return broadcast;
        }
        public static bool CheckNetMask(this ISocketUri uri, byte[] mask)
        {
            return IPAddressWithMask.CheckNetMask(mask);
        }
    }
}
