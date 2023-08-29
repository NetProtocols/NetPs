namespace NetPs.Socket
{
    using System;
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
