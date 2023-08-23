namespace NetPs.Tcp
{
    using System;

    public interface ITcpServerConfig : ITcpAccept, ITcpConfig, ITcpReceive
    {
        string BandAddress { get; }
    }
}
