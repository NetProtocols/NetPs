using NetPs.Socket.Packets;
using NetPs.Tcp;
using NetPs.Tcp.Hole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    public class HoleServer : IDisposable, IHoleEvents
    {
        public TcpHoleServer server { get; set; }
        public HoleServer(string uri)
        {
            server = new TcpHoleServer();
            server.BindEvents(this);
            server.Run(uri);
            Console.WriteLine($"run at {server.Address}");
        }

        public void Dispose()
        {
        }

        public void OnReceivedPacket(HolePacket packet, TcpClient client)
        {
            Console.WriteLine($"{client.RemoteIPEndPoint} send packet {packet.Operation.ToString()}");
        }
    }
}
