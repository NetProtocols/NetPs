namespace NetPs.Socket
{
    using System;

    /// <summary>
    /// 流队列
    /// </summary>
    public interface IQueueStream
    {
        /// <summary>
        /// 队列为空
        /// </summary>
        bool IsEmpty { get; }
        /// <summary>
        /// 数据长度
        /// </summary>
        long Length { get; }
        /// <summary>
        /// 可读性
        /// </summary>
        bool CanRead { get; }
        /// <summary>
        /// 可写性
        /// </summary>
        bool CanWrite { get; }
        /// <summary>
        /// 清空队列
        /// </summary>
        void Clear();
        /// <summary>
        /// 推入
        /// </summary>
        void Enqueue(byte[] block, int offset, int length);
        /// <summary>
        /// 退出
        /// </summary>
        int Dequeue(byte[] block, int offset, int length);
    }
}
