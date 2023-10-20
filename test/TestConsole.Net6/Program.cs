using NetPs.Socket;
using NetPs.Socket.Eggs;
using NetPs.Socket.Packets;
using NetPs.Tcp;
using NetPs.Tcp.Hole;
using NetPs.Udp.Hole;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using NetPs.Udp.Wol;
using NetPs.Socket.Extras.Security.MessageDigest;
using System.Text;
using NetPs.Socket.Memory;
using NetPs.Socket.Extras.Security.SecureHash;

namespace TestConsole.Net6
{
    internal class Program : IHeatingWatch
    {
        static void Main(string[] args)
        {
            var test_t = "a";
            new SecurityTest();
            var md = new SHA0();
            //var text = md.Make(Encoding.ASCII.GetBytes(test_t.Substring(0, 1000000)));
            var text = md.Make(Encoding.ASCII.GetBytes(test_t));
            Task.Factory.StartNew(() => Food.Heating()).Wait();
            new TcpRepeaterTest("172.17.0.161:15244", "127.0.0.1:7070");
            new UdpRepeaterTest("114.114.114.114:53", "127.0.0.1:7071");
            Utils.OpenBrowser_Edge("http://127.0.0.1:7070");
            //new UdpTest();
            new DnsTest("127.0.0.1:7071");
            //new DnsTest();
            new WolTest();
            //new PingTest();
            //Console.ReadLine();
            //var addr = "[::1]:9999";
            //var s = new UdpHoleServer();
            //s.Run(addr);
            //var c1 = new UdpHoleClient();
            //c1.Run("[::]:0");
            //c1.Connect(addr);
            //c1.Rg.Register("c1", "1234");
            //Thread.Sleep(100);
            //var c2 = new UdpHoleClient();
            //c2.Run("[::]:0");
            //c2.Connect(addr);
            //c2.He.Holed += He_Holed;
            //c1.Received += C1_Received;
            //c2.He.Hole("c1", "1234");
            //Console.ReadLine();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.Assert(false);
        }
        private static int i = 0;
        private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved();
            if (e.Exception.InnerException is SocketException sockerr)
            {
                Console.WriteLine(i);
            }
            i++;
            if (sender is IAsyncResult task)
            {
                task.AsyncWaitHandle.Close();
            }
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