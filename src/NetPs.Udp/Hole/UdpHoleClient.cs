namespace NetPs.Udp.Hole
{
    using NetPs.Socket.Packets;
    using System;
    public class UdpHoleClient : UdpHost
    {
        private bool holed = false;
        private IHoleEvents events { get; set; }
        public UdpHoleClient() : base()
        {
            this.ReceicedObservable.Subscribe(data =>
            {
                if (!holed)
                {
                    var packet = new HolePacket();
                    packet.Read(data.Data);
                    this.events?.OnReceivedPacket(packet);
                }
            });
            this.Rx.StartReveice();
        }

        public virtual bool IsHoled => this.holed;
        public virtual string Id { get; private set; }
        public virtual string Key { get; private set; }
        public virtual string ServerAddr { get; private set; }
        public virtual void Register(string server, string id, string key)
        {
            this.Id = id;
            this.Key = key;
            this.ServerAddr = server;
            var packet = new HolePacket(HolePacketOperation.Register, this.Id, this.Key);
            var tx = GetTx(server);
            tx.Transport(packet.GetData());
        }

        public virtual void TellHoled()
        {
            this.holed = true;
        }

        public virtual void BindEvents(IHoleEvents events)
        {
            this.events = events;
        }
    }
}
