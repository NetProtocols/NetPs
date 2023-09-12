using NetPs.Socket;
using NetPs.Socket.Eggs;
using NetPs.Tcp;
using NetPs.Tcp.Hole;
using NetPs.Udp.Hole;
using System.Net;

namespace TestConsole.Net6
{
    internal class Program : IHeatingWatch
    {
        static void Main(string[] args) 
        {
            var server_addr = "127.0.0.1:9999";
            var s = new UdpHoleServer(server_addr);
            var a = new UdpHoleClientA(server_addr, "host01", "123456");
            Thread.Sleep(100);
            new UdpHoleClientB(server_addr, "host01", "123456");;
            Console.ReadLine();
        }

        public void Heat_End()
        {
        }

        public void Heat_Progress()
        {
        }
    }
}