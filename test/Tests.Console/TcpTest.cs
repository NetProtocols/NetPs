using NetPs.Tcp;
using System;
using System.Threading;

namespace TestsConsole
{
    internal class TcpTest : ITcpReceive
    {
        public TcpTest()
        {
            //Food.Heating();
            server = new TcpServer((w, client) =>
            {
                x++;
                client.StartReceive(this);
            });
            server.Run("0.0.0.0:8000");
            CreateClients();
        }
        static TcpServer server { get; set; }
        static int j = 0;
        static int x = 0;
        public void TcpReceive(byte[] data, ITcpClient tcp)
        {
            j++;
            tcp.Transport(data);
        }

        public static void CreateClients()
        {

            do
            {
                Console.Write("tcp-test> ");
                var len = Console.ReadLine();
                if (len == "")
                {
                    Console.WriteLine(x + " " + j + " " + server.Connects.Count);
                    continue;
                }
                else if (len == "exit")
                {
                    break;
                }

                var times = DateTime.Now;
                //while (DateTime.Now.Ticks - times.Ticks < TimeSpan.FromSeconds(5).Ticks)
                {
                    var i = 0;

                    for (; i < int.Parse(len); i++)
                    {
                        var client = new TcpClient(tcp =>
                        {
                            //tcp.Socket.SetSocketOption(System.Net.Sockets.SocketOptionLevel.Socket, System.Net.Sockets.SocketOptionName.Linger, new System.Net.Sockets.LingerOption(false, 0));
                        });
                        client.ConnectedObservable.Subscribe(c =>
                        {
                            client.StartReceive();
                            client.Transport(new byte[] { 1 });
                        });
                        client.ReceivedObservable.Subscribe(data =>
                        {
                            client.FIN();
                        });
                        //client.TransportedObservable.Subscribe(tx => client.Dispose());
                        client.Connect($"127.0.0.1:8000");
                    }
                    Thread.Sleep(100);
                }

                Console.WriteLine($"connect:{x} receive:{j} online:{server.Connects.Count}");
                Console.WriteLine($"press \"exit\" out.");
            } while (true);
        }
    }
}
