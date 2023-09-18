namespace NetPs.Socket.Icmp
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    public class PingV6Client : PingClient
    {
        internal PingV6Client()
        {
        }
        public PingV6Client(int timeout, int buffer_size) : base(timeout, buffer_size)
        {
        }

        protected override void BindTo()
        {
            Address = new InsideSocketUri(InsideSocketUri.UriSchemeICMP, new IPEndPoint(IPAddress.IPv6Any, 0));
            Socket = new_socket();
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, Timeout);
            this.Socket.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
            this.v6 = true;
        }
    }
}
