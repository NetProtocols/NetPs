using System;
using System.Collections.Generic;
using System.Text;

namespace NetPs.Tcp.Interfaces
{
    public interface IEndTransport
    {
        void WhenTransportEnd(IDataTransport transport);
    }
}
