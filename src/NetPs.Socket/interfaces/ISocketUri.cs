﻿namespace NetPs.Socket
{
    using System;
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
    }
}
