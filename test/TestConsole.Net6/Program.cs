﻿using NetPs.Tcp;

namespace TestConsole.Net6
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var host = new TcpServer((s, c) =>
                {
                    c.StartMirror("172.17.0.161:5244", 1<<20);
                });
                host.Run("0.0.0.0:2302");
                Console.ReadLine();
                host.Dispose();
            }
        }
    }
}