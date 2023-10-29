using NetPs.Udp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    internal class UdpRepeaterTest : IDisposable
    {
        private UdpHost host { get; set; }
        private string mirror_address { get; set; }
        public UdpRepeaterTest(string src, string dst)
        {
            this.mirror_address = src;
            this.host = new UdpHost(dst);
            this.host.Rx.NoBufferReceived += (buffer, length, address) =>
            {
                var c = this.host.Clone(address);
                c.StartHub(new UdpMirrorHub(c, mirror_address, 10 << 20));
                return false;
            };
            this.host.StartReceive();
        }

        public void Dispose()
        {
            this.host.Dispose();
        }
    }
}
