using NetPs.Socket;
using NetPs.Socket.Eggs;
using NetPs.Socket.Packets;
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
            var addr = "[::1]:9999";
            var s = new UdpHoleServer();
            s.Run(addr);
            var c1 = new UdpHoleClient();
            c1.Run("[::]:0");
            c1.Connect(addr);
            c1.Rg.Register("c1", "1234");
            Thread.Sleep(100);
            var c2 = new UdpHoleClient();
            c2.Run("[::]:0");
            c2.Connect(addr);
            c2.He.Holed += He_Holed;
            c1.Received += C1_Received;
            c2.He.Hole("c1", "1234");
            //new TcpReapterTest("172.17.0.1:80", "0.0.0.0:3021");
            Console.ReadLine();
        }

        private static void C1_Received(NetPs.Udp.UdpData data)
        {
        }

        private static void He_Holed(UdpHoleCore core)
        {
            core.Tx.Transport(new byte[] { 1, 2, 3, 4, 5 });
        }

        public void Heat_End()
        {
        }

        public void Heat_Progress()
        {
        }
    }
}