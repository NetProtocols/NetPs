using NetPs.Socket;
using NetPs.Socket.Eggs;
using NetPs.Tcp;

namespace TestConsole.Net6
{
    internal class Program : IHeatingWatch
    {
        static void Main(string[] args)
        {
            Food.Heating();
            while (true)
            {
                var host = new TcpServer((s, c) =>
                {
                    c.StartMirror("172.17.0.161:5244", 10<<20); //20M 带宽
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