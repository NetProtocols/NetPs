using NetPs.Socket;
using NetPs.Udp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    public class UdpTest : IDisposable
    {
        byte[] testdata = new byte[4096];
        UdpHost host { get; set; }
        UdpHost host2 { get; set; }
        public UdpTest()
        {
            host = new UdpHost("127.0.0.1:2401");
            host.Rx.Received += Rx_Received;
            host.Rx.StartReceive();
            host2 = new UdpHost("0.0.0.0:0");
            var tx = host.GetTx("127.0.0.1:2401");
            tx.Transport(testdata);
            var tx2 = host2.GetTx("127.0.0.1:2401");
            tx2.Transport(testdata);
        }

        private void Rx_Received(UdpData data)
        {
            Debug.Assert(data.Data != testdata);
        }

        public void Dispose()
        {
            host.Dispose();
            host2.Dispose();
        }
    }
}
