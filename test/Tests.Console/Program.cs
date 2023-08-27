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
            var server = new TcpServer((_, client) =>
            {
                client.StartMirror("172.17.0.161:15244");
            });
            server.Run("127.0.0.1:15244");
            Console.ReadLine();
        }
    }
}
