namespace NetPs.Tcp.Hole
{
    using System;
    using NetPs.Socket;
    using NetPs.Socket.Packets;

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
        private IHoleEvents events { get; set; }
        public TcpHoleClient(string id, string key) : base()
        {
            this.Id = id;
            this.Key = key;
        }

        public TcpHoleClient() : base()
        {
        }

        public virtual void BindEvents(IHoleEvents events)
        {
            this.events = events;
        }
        public virtual TcpClient Clone()
        {
            var client = new TcpClient(core =>
            {
                core.SetReuseAddress(true);
                core.SetLinger(false, 0);
            });
            client.Bind(Address);
            return client;
        }

        protected override void OnConfiguration()
        {
            base.OnConfiguration();
            SetReuseAddress(true);
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
            client.BindRxEvents(this);
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
            this.events?.OnReceivedPacket(packet, (rx as TcpRx).Core as TcpClient);
            //switch (packet.Operation)
            //{
            //    case HolePacketOperation.CheckId:
            //        if (packet.Id == this.Id && packet.Key == this.Key)
            //        {
            //            ;
            //        }
            //        break;
            //}
        }

        public void OnDisposed(IRx rx)
        {
        }
    }
}
