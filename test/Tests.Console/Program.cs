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
            //new TcpTest();
            var server = new TcpServer((_, client) =>
            {
                client.StartMirror("172.17.0.161:5244");
            });
            server.Run("127.0.0.1:5244");
            Console.ReadLine();
        }
    }
}
