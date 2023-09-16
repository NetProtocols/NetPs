namespace NetPs.Socket
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public interface ISocketUri
    {
        string Scheme { get; }
        IPAddress IP { get; }
        string Host { get; }
        int Port { get; }

        bool Equal(IPEndPoint iPEndPoint);
        bool Equal(ISocketUri host);
        string ToString();
    }

    public static class ISocketUriExtra
    {
        public static bool IsIpv6(this ISocketUri uri)
        {
            return uri.IP.AddressFamily is AddressFamily.InterNetworkV6;
        }
        public static bool IsIpv4(this ISocketUri uri)
        {
            return uri.IP.AddressFamily is AddressFamily.InterNetwork;
        }
    }
}
