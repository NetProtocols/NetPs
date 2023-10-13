using NetPs.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    public class TcpRepeaterTest : IDisposable
    {
        private TcpServer host { get; set; }
        public TcpRepeaterTest(string src, string dst)
        {
            host = new TcpServer((s, c) =>
            {
                c.StartHub(new TcpMirrorHub(c, src, 10 << 20)); //1000M 带宽
            }, core =>
            {
                core.SetLinger(false, 0); //立即close
            });
            host.Run(dst);
        }

        public void Dispose()
        {
            this.host.Dispose();
        }
    }
}
