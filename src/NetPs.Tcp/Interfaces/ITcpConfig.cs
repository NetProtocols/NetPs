namespace NetPs.Tcp
{
    using System;

    public interface ITcpConfig
    {
        void TcpConfigure(TcpCore core);
    }
}
