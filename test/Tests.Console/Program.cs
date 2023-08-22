using NetPs.Socket;
using NetPs.Tcp;
using NetPs.Udp;
using NetPs.Udp.DNS;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestsConsole;

namespace TestsConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //new TcpServer((server, client) =>
            //{
            //    client.StartMirror("172.17.0.161:5244");
            //}).Run("0.0.0.0:5244", () =>
            //{
            //    Environment.Exit(0);
            //});
            var host = "nuget.org";
            Console.WriteLine(DnsHost.DNS_OPENDNS);
            Console.Write("> ");
            host = Console.ReadLine();
            while (host != "exit")
            {
                await HostResolver(host, DnsHost.DNS_OPENDNS);

                Console.Write("> ");
                host = Console.ReadLine();
            }
        }

        static internal async Task HostResolver(string host, string dns, int port= 53)
        {
            dns += ":" + port;
            try
            {
                using (var dns_client = new DnsHost(1000))
                {
                    var sw = new Stopwatch();

                    sw.Start();
                    var packet = await dns_client.SendReqA(dns, host);
                    sw.Stop();
                    var times = sw.ElapsedMilliseconds;
                    sw.Reset();
                    sw.Start();
                    var packet2 = await dns_client.SendReqAAAA(dns, host);
                    sw.Stop();
                    var times2 = sw.ElapsedMilliseconds;
                    //var times2 = sw.ElapsedMilliseconds;
                    var addrs = packet.Answers.Where(a => a.Address !=null).Select(a => a.Address.ToString()).ToArray();
                    var addrs2 = packet2.Answers.Where(a => a.Address != null).Select(a => a.Address.ToString()).ToArray();
                    var cnames = packet.Answers.Where(a => a.CNAME != null).Select(a => a.CNAME).ToArray();
                    Console.WriteLine($"IPv4 take {times}ms; Total: " + addrs.Length);
                    Console.WriteLine($"IPv6 take {times2}ms; Total: " + addrs2.Length);
                    Console.Write(host + " [IPv4]: ");
                    Console.WriteLine(string.Join(";", addrs));
                    Console.Write(host + " [IPv6]: ");
                    Console.WriteLine(string.Join(";  ", addrs2));
                    Console.Write(host + " [CNAME]: ");
                    Console.WriteLine(string.Join(";  ", cnames));
                }
            }
            catch
            {
                Console.WriteLine($"Error Host: [{host}].");
            }
            finally
            {
                Console.WriteLine("enter 'exit' to exit.");
            }
        }
    }
}
