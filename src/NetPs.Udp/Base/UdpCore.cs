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

        public override void Bind()
        {
            this.OnConfiguration();
            base.Bind();
        }

        protected virtual void OnConfiguration()
        {
            this.Socket = new_socket();
        }

        public virtual void Bind(string address)
        {
            this.Bind(new InsideSocketUri(InsideSocketUri.UriSchemeUDP, address));
        }

        protected override void OnClosed()
        {
        }

        protected override void OnLosed()
        {
        }
    }
}
