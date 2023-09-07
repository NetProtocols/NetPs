namespace NetPs.Tcp.Hole
{
    using NetPs.Socket;
    using System;

    /// <summary>
    /// 内网穿透客户端
    /// </summary>
    /// <remarks>
    /// * 需要管理员权限
    /// </remarks>
    public class TcpHoleClient : TcpServer, ITcpClientEvents, IRxEvents
    {
        public virtual string Id { get; private set; }
        public virtual string Key { get; private set; }
        public TcpHoleClient(string id, string key) : base()
        {
            this.Id = id;
            this.Key = key;
        }

        public TcpHoleClient() : base()
        {
        }

        public virtual TcpClient Clone()
        {
            var client = new TcpClient(core =>
            {
                core.AllowReuseAddress();
            });
            client.Bind(Address);
            return client;
        }

        protected override void OnConfiguration()
        {
            base.OnConfiguration();
            AllowReuseAddress();
        }

        protected override void OnAccepted(object client)
        {
            base.OnAccepted(client);
            if (client is TcpClient tcpClient)
            {
                tcpClient.BindRxEvents(this);
                tcpClient.StartReceive();
            }
        }

        public TcpClient client { get; set; }
        public virtual void Register(string server)
        {
            client = this.Clone();
            client.BindEvents(this);
            client.Connect(server);
        }


        public void OnConfiguration(ITcpClient client)
        {
        }

        public void OnConnected(ITcpClient client)
        {
            var packet = new HolePacket(HolePacketOperation.Register, this.Id, this.Key);
            client.StartReceive();
            client.Transport(packet.GetData());
        }

        public void OnDisconnected(ITcpClient client)
        {
        }

        public void OnClosed(ITcpClient client)
        {
        }

        public void OnLosed(ITcpClient client)
        {
        }

        public void OnConfiguration(TcpCore core)
        {
        }

        public void OnReceiving(IRx rx)
        {
        }

        public void OnReceived(IRx rx)
        {
            var packet = new HolePacket();
            var len = packet.Read(rx.Buffer);
            switch (packet.Operation)
            {
                case HolePacketOperation.CheckId:
                    if (packet.Id == this.Id && packet.Key == this.Key)
                    {
                        ;
                    }
                    break;
            }
        }

        public void OnDisposed(IRx rx)
        {
        }
    }
}
