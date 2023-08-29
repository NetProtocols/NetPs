namespace TestsConsole
{
    using NetPs.Tcp;
    using System;

    class Program
    {
        static void Main()
        {
            var host = new TcpServer((s, c) =>
            {
                c.StartMirror("172.17.0.161:5244");
            });
            host.Run("0.0.0.0:5244");
            Console.ReadLine();
        }
    }
}
