using NetPs.Socket;
using NetPs.Socket.Eggs;
using NetPs.Socket.Icmp;
using NetPs.Socket.Packets;
using NetPs.Udp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    internal class PingTest
    {
        PingClient ping;
        PingV6Client pingV6;
        public PingTest()
        {
            ping = new PingClient(3000, 8192);
            pingV6 = new PingV6Client(3000, 8192);
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
                    if (InsideSocketUri.IsIPAddress(host))
                    {
                        InsideSocketUri.ParseIPAddress(host);
                    }
                    else
                    {
                        var dns_packet = await new DnsHost(1000, 1).SendReqA($"{DnsHost.DNS_ALI}:53", host);
                        host = dns_packet.Answers.Where(a => a.Address != null).Select(a => a.Address.ToString()).FirstOrDefault();
                    }

                    if (string.IsNullOrEmpty(host)) continue;

                    var packet = new PingPacket(PingPacket.DATA_32, PingPacketKind.Request)
                    {
                        Address = IPAddress.Parse(host)
                    };
                    var sw = new Stopwatch();

                    IPingPacket p;
                    if (packet.Address.IsIpv4())
                    {
                        sw.Start();
                        p = await ping.Ping(packet);
                        sw.Stop();
                    }
                    else
                    {
                        sw.Start();
                        p = await pingV6.Ping(packet);
                        sw.Stop();
                    }
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
