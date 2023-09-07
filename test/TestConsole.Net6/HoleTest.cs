using NetPs.Socket;
using NetPs.Tcp;
using NetPs.Tcp.Hole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    public class HoleTest : IDisposable, IRxEvents
    {
        TcpHoleClient holeClient { get; set; }
        TcpHoleClient holeClient2 { get; set; }
        TcpClient client { get; set; }
        TcpClient client2 { get; set; }
        string id { get; set; }
        string key { get; set; }
        public HoleTest(string id, string key)
        {
            this.id = id;
            this.key = key;
            holeClient = new TcpHoleClient(id, key);
            holeClient.Run("0.0.0.0:0");
            holeClient.Register("127.0.0.1:9999");
            holeClient2 = new TcpHoleClient();
            holeClient2.Run("0.0.0.0:0");
            //holeClient2.Register("192.168.68.140:9999");
            client = holeClient2.Clone();
            client.BindRxEvents(this);
            client.Connected += Client_Connected;
            client.Connect("192.168.68.140:9999");
        }

        private void Client_Connected(object source)
        {
            client.StartReceive();
            var packet = new HolePacket(HolePacketOperation.GetId, this.id, "");
            client.Transport(packet.GetData());

        }

        public void Dispose()
        {
        }

        private string ConsoleRead(string tip)
        {
            Console.Write(tip);
            var a = Console.ReadLine();
            return a;
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
                case HolePacketOperation.GetId:
                    client2 = holeClient2.Clone();
                    client2.ConnectedObservable.Subscribe(o =>
                    {
                        client2.StartReceive();
                        var packet = new HolePacket(HolePacketOperation.CheckId, this.id, this.key);
                        client2.Transport(packet.GetData());
                    });
                    Console.WriteLine(packet.Address);
                    client2.Connect(new SocketUri(SocketUri.UriSchemeNetTcp, packet.Address));
                    break;
            }
        }

        public void OnDisposed(IRx rx)
        {
        }
    }
}
