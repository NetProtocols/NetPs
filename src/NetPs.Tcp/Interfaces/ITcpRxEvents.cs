namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;

    /// <summary>
    /// Tcp 数据接收事件集
    /// </summary>
    public interface ITcpRxEvents : IRxEvents
    {
        /// <summary>
        /// 收到FIN
        /// </summary>
        /// <param name="rx"></param>
        void OnShutdown(ITcpRx rx);
    }
}
