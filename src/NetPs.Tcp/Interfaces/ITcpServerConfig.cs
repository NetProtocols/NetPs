namespace NetPs.Tcp
{
    using System;

    /// <summary>
    /// Tcp Server启动配置
    /// </summary>
    /// <remarks>初始化参数</remarks>
    public interface ITcpServerConfig : ITcpAccept, ITcpReceive, ITcpConfig
    {
        string BandAddress { get; }
    }
}
