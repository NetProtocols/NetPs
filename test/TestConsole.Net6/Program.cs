using NetPs.Socket;
using NetPs.Socket.Eggs;
using NetPs.Tcp;
using NetPs.Tcp.Hole;
using System.Net;

namespace TestConsole.Net6
{
    internal class Program : IHeatingWatch
    {
        static void Main(string[] args) 
        {
            //new HoleServerTest("0.0.0.0:9999");
            new HoleTest("host01", "123456");
            Console.ReadLine();
            IDisposable test;
            //test = new TcpReapterTest("172.17.0.161:5244", "127.0.0.1:2303");
            test = new UdpTest();
            using (test)
            {
                Console.ReadLine();
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