using NetPs.Socket.Packets;
using NetPs.Udp.Hole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    internal class UdpHoleClientA: IHoleEvents
    {
        private UdpHoleClient client { get; set; }
        public UdpHoleClientA(string server, string id, string key)
        {
            client = new UdpHoleClient();
            client.BindEvents(this);
            client.Rx.Received += Rx_Received;
            client.Register(server, id, key);
        }

        private void Rx_Received(NetPs.Udp.UdpData data)
        {
            Console.WriteLine(data.IP);
        }

        public void OnReceivedPacket(HolePacket packet)
        {
            switch (packet.Operation)
            {
                case HolePacketOperation.Hole:
                case HolePacketOperation.HoleCallback:
                    client.TellHoled();
                    Task.Factory.StartNew(() =>
                    {
                        var data = new byte[] { 6, 4, 3, 2, 1 };
                        while (true)
                        {
                            var tx = client.GetTx(packet.Address);
                            tx.Transport(data);
                            Thread.Sleep(500);
                        }
                    });
                    break;
            }
        }
    }
}
