using NetPs.Socket;
using NetPs.Socket.Eggs;
using NetPs.Tcp;
using NetPs.Tcp.Hole;
using System.Runtime.CompilerServices;

namespace TestConsole.Net6
{
    internal class Program : IHeatingWatch
    {
        static void Main(string[] args)
        {
            //var hole_server = new TcpHoleServer();
            //hole_server.Run("0.0.0.0:9999");
            //Console.ReadLine();
            //Food.Heating();
            while (true)
            {
                var host = new TcpServer((s, c) =>
                {
                    c.StartMirror("172.17.0.161:5244", 10 << 20); //20M 带宽
                }, core =>
                {
                    core.AllowReuseAddress();
                });
                host.Run("0.0.0.0:2303");
                Console.ReadLine();
                host.Dispose();
            }
        }

        public void Heat_End()
        {
        }

        public void Heat_Progress()
        {
        }
    }
}