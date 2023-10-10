using System;
using System.Collections.Generic;
using System.Text;

namespace NetPs.Udp
{
    public abstract class BindUdpCore
    {
        public virtual UdpCore Core { get; private set; }
        public virtual void BindCore(UdpCore core)
        {
            this.Core = core;
        }
    }

    public interface IBindUdpCore
    {
        UdpCore Core { get; }
        void BindCore(UdpCore core);
    }
}
