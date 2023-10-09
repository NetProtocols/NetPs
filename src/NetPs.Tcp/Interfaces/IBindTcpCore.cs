namespace NetPs.Tcp
{
    using System;
    public interface IBindTcpCore
    {
        TcpCore Core { get; }

        void BindCore(ITcpClient core);
        void BindCore(TcpCore core);
    }
}
