using NetPs.Socket;
using NetPs.Udp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    public class UdpTest : IDisposable,IRxEvents
    {
        UdpHost host { get; set; }
        UdpHost host2 { get; set; }
        public UdpTest()
        {
            host = new UdpHost("127.0.0.1:2401");
            host.Rx.BindEvents(this);
            host.Rx.StartReveice();
            host2 = new UdpHost();
            var tx = host2.GetTx("127.0.0.1:2401");
            tx.Transport(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, }, 0, -1);

        }

        public void Dispose()
        {
            host.Dispose();
            host2.Dispose();
        }

        public void OnReceiving(IRx rx)
        {
        }

        public void OnReceived(IRx rx)
        {
            Console.Write(rx.Buffer);
        }

        public void OnDisposed(IRx rx)
        {
        }
    }
}
