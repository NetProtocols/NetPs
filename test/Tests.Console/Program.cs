namespace TestsConsole
{
    using NetPs.Socket.Eggs;
    using NetPs.Tcp;
    using NetPs.Udp;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    class Program : ITcpReceive
    {
        static void Main()
        {
            var prog = new Program();
            //Food.Heating();
            server = new TcpServer((w, client) =>
            {
                x++;
                client.StartReceive(prog);
            });
            server.Run("0.0.0.0:8000");
            CreateClients();
        }
        static TcpServer server { get; set; }
        static int j = 0;
        static int x = 0;
        public void TcpReceive(byte[] data, TcpClient tcp)
        {
            j++;
            tcp.Transport(data);
        }

        public static void CreateClients()
        {

            do
            {
                var len = Console.ReadLine();
                if (len == "")
                {
                    Console.WriteLine(x + " " + j + " " + server.Connects.Count);
                    continue;
                }

                //var times = DateTime.Now;
                //while (DateTime.Now.Ticks - times.Ticks < TimeSpan.FromSeconds(5).Ticks)
                {
                    var i = 0;

                    for (; i < int.Parse(len); i++)
                    {
                        var client = new TcpClient();
                        client.ConnectedObservable.Subscribe(c =>
                        {
                            client.StartReceive();
                            client.Transport(new byte[] { 1 });
                        });
                        client.ReceivedObservable.Subscribe(data =>
                        {
                            client.Dispose();
                        });
                        //client.TransportedObservable.Subscribe(tx => client.Dispose());
                        client.Connect($"127.0.0.1:8000");
                    }
                    Thread.Sleep(100);
                }

                Console.WriteLine(x + " " + j + " " + server.Connects.Count);
            } while (true);
        }
    }
}
