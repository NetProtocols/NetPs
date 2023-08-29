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

    class Program
    {
        static void Main()
        {
            int i = 0;
            //new TcpTest();
            var server = new TcpServer((_, client) =>
            {
                i++;
                client.StartMirror("172.17.0.161:5244");
            });
            server.Run("127.0.0.1:5301");
            var server2 = new TcpServer((_, client) =>
            {
                client.StartMirror("127.0.0.1:5301");
            });
            server2.Run("0.0.0.0:5244");
            while (true)
            {
                Console.ReadLine();
                Console.WriteLine(i+" " + TcpTx.i);
            }
        }
    }
}
