namespace NetPs.Tcp
{
    using System;
    /// <summary>
    /// 接收等待
    /// </summary>
    public interface IReceiveWait
    {
        void ReceiveWait(TcpRx rx);
    }
}
