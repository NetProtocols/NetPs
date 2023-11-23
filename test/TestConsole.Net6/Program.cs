using System;
using NetPs.Socket;
using NetPs.Socket.Eggs;

namespace TestConsole.Net6
{
    internal class Program : IHeatingWatch
    {
        static void Main(string[] args)
        {
            Task.Factory.StartNew(() => Food.Heating()).Wait();
            new SecurityTest();
            new TcpRepeaterTest("172.17.0.161:15244", "127.0.0.1:7070");
            new UdpRepeaterTest("114.114.114.114:53", "127.0.0.1:7071");
            Utils.OpenBrowser_Edge("http://127.0.0.1:7070");
            //new UdpTest();
            new DnsTest("127.0.0.1:7071");
            //new DnsTest();
            new WolTest();
            //new PingTest();
        }

        public void Heat_End()
        {
        }

        public void Heat_Progress()
        {
        }
    }
}