namespace NetPs.Socket
{
    using System;

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
        void Enqueue(byte[] block, int offset, int length);
        int Dequeue(byte[] block, int offset, int length);
    }
}
