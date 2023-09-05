namespace NetPs.Tcp
{
    using System;

    /// <summary>
    /// Tcp 数据发送事件集
    /// </summary>
    public interface ITcpTxEvents
    {
        /// <summary>
        /// 开始发送
        /// </summary>
        /// <param name="tx"></param>
        void OnTransporting(ITcpTx tx);
        /// <summary>
        /// 当发送队列被添加
        /// </summary>
        /// <param name="tx"></param>
        void OnTransportEnqueue(ITcpTx tx);
        /// <summary>
        /// 单次发送完成
        /// </summary>
        /// <param name="tx"></param>
        void OnBufferTransported(ITcpTx tx);
        /// <summary>
        /// 发送完成
        /// </summary>
        /// <param name="tx"></param>
        void OnTransported(ITcpTx tx);
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="rx"></param>
        void OnDisposed(ITcpTx rx);
    }
}
