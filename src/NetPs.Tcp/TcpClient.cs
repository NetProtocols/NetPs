namespace NetPs.Tcp
{
    using System;
    using System.Net.Sockets;

    public class TcpClient: TcpClientFactory<TcpTx, TcpRx>
    {
        public TcpClient(TcpConfigFunction tcp_config = null) : base(tcp_config) { }
        public TcpClient(ITcpClientEvents events) : base(events) { }

        public TcpClient(Socket socket) : base(socket) { }

        public IHub Hub { get; set; }
        /// <summary>
        /// 镜像模式.
        /// </summary>
        /// <param name="address">镜像来源.</param>
        public void StartMirror(string address, int limit = -1)
        {
            if (Hub != null) Hub.Close();
            Hub = new MirrorHub(this, address, limit);
            Hub.Start();
        }

        protected override void OnClosed()
        {
            if (Hub != null) Hub.Close();
            base.OnClosed();
        }
    }
}
