using NetPs.Socket.Icmp;
using NetPs.Udp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestsConsole
{
    internal class PingTest
    {
        PingClient ping;
        public PingTest()
        {
            ping = new PingClient(3000, 8192);
            Run().Wait();
        }

        public async Task Run()
        {
            while (true)
            {
                Console.Write("ping> ");
                var host = Console.ReadLine();
                if (host == "exit") break;
                try
                {
                    if (string.IsNullOrEmpty(host)) continue;
                    try
                    {
                        IPAddress.Parse(host);
                    }
                    catch
                    {
                        var dns_packet = await new DnsHost(1000, 1).SendReqA($"{DnsHost.DNS_ALI}:53", host);
                        host = dns_packet.Answers.Where(a => a.Address != null).Select(a => a.Address.ToString()).FirstOrDefault();
                    }

                    if (string.IsNullOrEmpty(host)) continue;

                    var packet = new PingPacket(new byte[56], PingPacketKind.Request)
                    {
                        Address = IPAddress.Parse(host)
                    };
                    var sw = new Stopwatch();

                    sw.Start();

                    var p = await ping.Ping(packet);
                    sw.Stop();
                    var times2 = sw.ElapsedMilliseconds;
                    Console.Write(p.Address.ToString());
                    Console.Write("  ");
                    Console.Write(times2);
                    Console.Write("ms");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(host);
                    Console.WriteLine(ex.Message);
                }
                Console.WriteLine();
            }
        }
    }
}
