namespace NetPs.Tcp
{
    using System;
    internal interface IBindTcpCore
    {
        TcpCore Core { get; }

        void BindCore(TcpCore core);
    }
}
