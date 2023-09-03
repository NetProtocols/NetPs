namespace NetPs.Tcp
{
    using System;

    public interface ITcpConfig
    {
        void OnConfiguration(TcpCore core);
    }
}
