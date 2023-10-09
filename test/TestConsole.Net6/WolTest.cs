using NetPs.Udp.Wol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Net6
{
    internal class WolTest
    {
        private WolSender wol { get; }
        public WolTest(string addr)
        {
            this.wol = new WolSender();
            wol.Send(addr);
        }
    }
}
