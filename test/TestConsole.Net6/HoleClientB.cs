using NetPs.Socket;
using NetPs.Socket.Packets;
using NetPs.Tcp;
using NetPs.Tcp.Hole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    public class HoleClientB : IDisposable, IRxEvents
    {
        TcpHoleClient holeClient { get; set; }
        TcpClient client { get; set; }
        string id { get; set; }
        string key { get; set; }
        private TcpClient raw_client;
        private string server;
        public HoleClientB(string server, string id, string key)
        {
            this.id = id;
            this.key = key;
            this.server = server;
            holeClient = new TcpHoleClient();
            holeClient.AcceptClientObservable.Subscribe(c =>
            {
                c.StartReceive();
                Console.WriteLine("B Accept");
            });
            holeClient.Run("0.0.0.0:0");
            Console.WriteLine($"B run at{holeClient.Address}");
            client = holeClient.Clone();
            client.BindRxEvents(this);
            client.Connected += Client_Connected;
            client.Connect(server);
        }

        private void Client_Connected(object source)
        {
            client.StartReceive();
            var packet = new HolePacket(HolePacketOperation.Hole, this.id, "");
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
                case HolePacketOperation.Hole:
                    //client.Close();
                    Task.Factory.StartNew(() =>
                    {
                        raw_client = holeClient.Clone();
                        raw_client.Connected += Raw_client_Connected;
                        raw_client.DisConnected += Raw_client_Closed;
                        ip = packet.Address;
                        raw_client.Connect(new SocketUri(SocketUri.UriSchemeNetTcp, ip));
                        Console.WriteLine($"{client.Address} connect {packet.Address}");
                    });
                    break;
            }
        }

        private void Raw_client_Connected(object source)
        {
            Console.WriteLine($"{raw_client.Address} Connect");
            raw_client.StartReceive();
            var packet = new HolePacket(HolePacketOperation.CheckId, this.id, this.key);
            raw_client.Transport(packet.GetData());
        }

        private IPEndPoint ip;
        private void Raw_client_Closed(object source)
        {
            Console.WriteLine($"{raw_client.Address} Lose");
        }

        public void OnDisposed(IRx rx)
        {
        }
    }
}
