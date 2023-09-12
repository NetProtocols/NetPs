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
    public class HoleClientA : IDisposable, IHoleEvents, ITcpServerEvents
    {
        public TcpHoleClient holeClient { get; set; }
        string id { get; set; }
        string key { get; set; }
        private TcpClient raw_client;
        private string server;
        public HoleClientA(string server, string id, string key)
        {
            this.id = id;
            this.key = key;
            this.server = server;
            holeClient = new TcpHoleClient(id, key);
            holeClient.BindEvents(this);
            holeClient.AcceptClientObservable.Subscribe(c =>
            {
                c.StartReceive();
                Console.WriteLine("A Accept");
            });
            holeClient.Run("0.0.0.0:0");
            Console.WriteLine($"A run at{holeClient.Address}");
            holeClient.BindEvents(this);
            holeClient.Register(server);

        }

        public void Dispose()
        {
            //this.holeClient.Dispose();
        }

        public void OnReceivedPacket(HolePacket packet, TcpClient client)
        {
            switch (packet.Operation)
            {
                case HolePacketOperation.Hole:
                case HolePacketOperation.HoleCallback:
                    //this.holeClient.client.Close();
                    Task.Factory.StartNew(() =>
                    {
                        raw_client = holeClient.Clone();
                        raw_client.Connected += Raw_client_Connected;
                        raw_client.DisConnected += Raw_client_Closed;
                        ip = packet.Address;
                        raw_client.Connect(new SocketUri(SocketUri.UriSchemeNetTcp, packet.Address));
                        Console.WriteLine($"{client.Address} connect {packet.Address}");
                    });
                    //client.Transport(packet.GetData());
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

        public void OnConfiguration(ITcpServer tcpServer)
        {
        }

        public void OnListened(ITcpServer tcpServer)
        {
        }

        public void OnAccepted(ITcpServer tcpServer, ITcpClient tcpClient)
        {
            tcpClient.BindEvents(holeClient);
            tcpClient.StartReceive();
        }

        public void OnSocketLosed(ITcpServer tcpServer, ITcpClient tcpClient)
        {
        }

        public void OnClosed(ITcpServer tcpServer)
        {
        }

        public void OnConfiguration(TcpCore core)
        {
        }
    }
}
