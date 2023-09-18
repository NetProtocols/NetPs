namespace NetPs.Socket
{
    using System;

    /// <summary>
    /// 接收事件
    /// </summary>
    public interface IRxEvents
    {
        /// <summary>
        /// 开始接收
        /// </summary>
        /// <param name="rx"></param>
        void OnReceiving(IRx rx);
        /// <summary>
        /// 接收到数据
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="data"></param>
        void OnReceived(IRx rx);
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="rx"></param>
        void OnDisposed(IRx rx);
    }
}
