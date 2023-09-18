namespace NetPs.Socket
{
    using NetPs.Socket.Eggs;
    using NetPs.Tcp;
    using System;

    /// <summary>
    /// 加热类
    /// </summary>
    public sealed class Heat_tcp : IHeat
    {
        public void Start(IHeatingWatch watch)
        {
            new TcpServer().Dispose();
            new TcpClient().Dispose();
            new TcpAx().Dispose();
            new TcpLimitRx().Dispose();
            new TcpLimitQueueTx().Dispose();
            new TcpRxRepeater().Dispose();
            new TcpRepeaterClient().Dispose();
            watch.Heat_Progress();
        }
    }
}
