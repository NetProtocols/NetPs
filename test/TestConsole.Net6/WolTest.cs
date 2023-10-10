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
        public WolTest()
        {
            this.wol = new WolSender();
            var text = string.Empty;
            do
            {
                Console.Write("wol> ");
                text = Console.ReadLine();
                if (WakeOnLanPacket.IsMacAddress(text))
                {
                    this.wol.Send(text);
                }
            } while (text != "exit");
        }
    }
}
