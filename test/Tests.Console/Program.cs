namespace TestsConsole
{
    using NetPs.Socket.Eggs;
    using NetPs.Udp;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    class Program : IHeatingWatch
    {
        static bool exited = false;

        static void Main()
        {
            Console.Write("loading");
            new Thread(new ThreadStart(() => Food.Heating(new Program()))).Start();
            while (!exited) Thread.Sleep(100);
        }

        public void Heat_Progress()
        {
            Console.Write(".");
        }

        public async void Heat_End()
        {
            Console.WriteLine("ok");
            var host = "nuget.org";
            var dns = DnsHost.DNS_NETEASE;
            Console.WriteLine(dns);
            Console.Write("> ");
            host = Console.ReadLine();
            while (host != "exit")
            {
                var a = host.Split(" ".ToArray(), 2);
                if (a.Length == 2)
                    switch (a[0])
                    {
                        case "dns":
                            dns = a[1];
                            break;

                    }
                else
                {
                    await HostResolver(host, dns);
                }

                Console.Write("> ");
                host = Console.ReadLine();
            }
            exited = true;
        }
        private static async Task HostResolver(string host, string dns, int port = 53)
        {
            dns += ":" + port;
            try
            {
                using (var dns_client = new DnsHost(5000))
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
                    var addrs = packet.Answers.Where(a => a.Address != null).Select(a => a.Address.ToString()).ToArray();
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
