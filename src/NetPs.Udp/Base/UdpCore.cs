namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class UdpCore : SocketCore
    {
        /// <summary>
        /// Gets or sets a value indicating whether 正在接收.
        /// </summary>
        public virtual bool Receiving { get; set; }

        public UdpCore()
        {

        }

        //绑定到指定地址
        public virtual void Bind(SocketUri address)
        {
            this.Address = address;
            this.IPEndPoint = new IPEndPoint(address.IP, address.Port);
            this.Socket = new Socket(address.IP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            this.Socket.Bind(this.IPEndPoint);
        }

        public virtual void Bind(string address)
        {
            this.Bind(new SocketUri(address));
        }
    }
}
