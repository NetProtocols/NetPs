namespace NetPs.Udp.Hole
{
    using NetPs.Socket.Packets;
    using System;
    using System.Collections.Generic;

    public class UdpHoleServer : UdpHost
    {
        private IHoleEvents events { get; set; }
        private Dictionary<string, HolePacket> hosts { get; set; }
        public UdpHoleServer(string addr) : base(addr)
        {
            this.hosts = new Dictionary<string, HolePacket>();
            this.ReceicedObservable.Subscribe(data =>
            {
                var packet = new HolePacket();
                packet.Read(data.Data);
                switch (packet.Operation)
                {
                    case HolePacketOperation.Register:
                        packet.Address = data.IP;
                        hosts[packet.Id] = packet;
                        break;
                    case HolePacketOperation.Hole:
                    case HolePacketOperation.HoleCallback:
                        packet.IsCallback = true;
                        //根据Address判断
                        if (hosts.ContainsKey(packet.Id))
                        {
                            var id_address = hosts[packet.Id].Address;
                            var tx = this.GetTx(id_address);
                            packet.Address = data.IP;
                            tx.Transport(packet.GetData());

                            packet.Address = id_address;
                            tx = this.GetTx(data.IP);
                            packet.Address = data.IP;
                            tx.Transport(packet.GetData());

                        }
                        break;
                }
                this.events?.OnReceivedPacket(packet);
            });
            this.Rx.StartReveice();
        }

        public virtual void BindEvents(IHoleEvents events)
        {
            this.events = events;
        }
    }
}
