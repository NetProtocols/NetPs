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
            if (address.Port == 0)
            {
                // 端口由socket 分配
                var ip = Socket.LocalEndPoint as IPEndPoint;
                if (ip != null)
                {
                    Address = new SocketUri($"{Address.Scheme}{SocketUri.SchemeDelimiter}{Address.Host}{SocketUri.PortDelimiter}{ip.Port}");
                    IPEndPoint.Port = ip.Port;
                }
            }
        }

        public virtual void Bind(string address)
        {
            this.Bind(new SocketUri(address));
            this.IsUdp();
        }

        protected override void OnConnected()
        {
        }

        protected override void OnClosed()
        {
        }
    }
}
