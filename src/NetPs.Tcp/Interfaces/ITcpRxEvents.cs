namespace NetPs.Tcp
{
    using System;

    /// <summary>
    /// Tcp 数据接收事件集
    /// </summary>
    public interface ITcpRxEvents
    {
        /// <summary>
        /// 开始接收
        /// </summary>
        /// <param name="rx"></param>
        void OnReceiving(ITcpRx rx);
        /// <summary>
        /// 接收到数据
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="data"></param>
        void OnReceived(ITcpRx rx);
        /// <summary>
        /// 收到FIN
        /// </summary>
        /// <param name="rx"></param>
        void OnShutdown(ITcpRx rx);
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="rx"></param>
        void OnDisposed(ITcpRx rx);
    }
}
