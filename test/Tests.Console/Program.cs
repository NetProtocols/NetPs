using NetPs.Socket;
using NetPs.Tcp;
using NetPs.Udp;
using NetPs.Udp.DNS;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TestsConsole;

namespace TestsConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            new TcpServer((server, client) =>
            {
                client.StartMirror("172.17.0.161:5244");
            }).Run("0.0.0.0:5244", () =>
            {
                Environment.Exit(0);
            });
            var packet = await new DnsHost().SendReqA("223.6.6.6:53", "nuget.org");
            Console.ReadKey();
        }
    }
}
