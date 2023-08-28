using System;
using System.Collections.Generic;
using System.Text;

namespace NetPs.Tcp.Interfaces
{
    /// <summary>
    /// 数据传输
    /// </summary>
    public interface IDataTransport : IDisposable
    {
        bool IsDisposed { get; }
        bool Running { get; }
        void Transport(byte[] data, int offset, int length);

        void LookEndTransport(IEndTransport endTransport);
    }
}
