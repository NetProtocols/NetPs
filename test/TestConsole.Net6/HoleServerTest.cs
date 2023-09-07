using NetPs.Tcp;
using NetPs.Tcp.Hole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    public class HoleServerTest : IDisposable, IHoleServerEvents
    {
        TcpHoleServer server { get; set; }
        public HoleServerTest(string uri)
        {
            server = new TcpHoleServer();
            server.BindEvents(this);
            server.Run(uri);
            Console.WriteLine($"run at {server.Address}");
        }

        public void Dispose()
        {
        }

        public void OnHoleReceived(HolePacket packet, TcpClient client)
        {
            Console.WriteLine($"{client.RemoteIPEndPoint} send packet {packet.Operation.ToString()}");
        }
    }
}
