namespace NetPs.Socket
{
    using System;
    /// <summary>
    /// 发送事件
    /// </summary>
    public interface ITxEvents
    {
        /// <summary>
        /// 开始发送
        /// </summary>
        /// <param name="tx"></param>
        void OnTransporting(ITx tx);
        /// <summary>
        /// 发送完成
        /// </summary>
        /// <param name="tx"></param>
        void OnTransported(ITx tx);
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="rx"></param>
        void OnDisposed(ITx rx);
    }
}
