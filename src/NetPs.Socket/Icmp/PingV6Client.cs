namespace NetPs.Socket.Icmp
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    public class PingV6Client : PingClient
    {
        public PingV6Client(int timeout, int buffer_size) : base(timeout, buffer_size)
        {
        }

        protected override void BindTo()
        {
#if NET35_CF
            Socket = new System.Net.Sockets.Socket(AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.Icmp);
#else
            Socket = new System.Net.Sockets.Socket(AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.IcmpV6);
#endif
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, Timeout);
            this.Socket.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
            this.v6 = true;
        }
    }
}
