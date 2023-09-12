using NetPs.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    public class TcpReapterTest : IDisposable
    {
        private TcpServer host { get; set; }
        public TcpReapterTest(string src, string dst)
        {
            host = new TcpServer((s, c) =>
            {
                c.StartMirror(src, 10 << 20); //100M 带宽
            }, core =>
            {
                core.SetReuseAddress(true);
            });
            host.Run(dst);
        }

        public void Dispose()
        {
            this.host.Dispose();
        }
    }
}
