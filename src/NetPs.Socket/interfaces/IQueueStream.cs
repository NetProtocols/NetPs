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
        byte[] Buffer { get; }
        long WritePosition { get; }
        long ReadPosition { get; }
        /// <summary>
        /// 清空队列
        /// </summary>
        void Clear();
        /// <summary>
        /// 推入
        /// </summary>
        void Enqueue(byte[] block, int offset, int length);
        /// <summary>
        /// 推出
        /// </summary>
        int Dequeue(byte[] block, int offset, int length);
        int RequestRead(int length);
        void RecordRead(int length);
    }

    public static class IQueueStreamExtra
    {
        /// <summary>
        /// 推入
        /// </summary>
        /// <remarks>
        /// 推入全部数据。offset: 0, length: -1
        /// </remarks>
        public static void Enqueue(this IQueueStream stream, byte[] block)
        {
            stream.Enqueue(block, 0, -1);
        }
        /// <summary>
        /// 推出
        /// </summary>
        /// <remarks>
        /// 推出全部数据。offset: 0, length: -1
        /// </remarks>
        public static int Dequeue(this IQueueStream stream, byte[] block)
        {
            return stream.Dequeue(block, 0, -1);
        }
    }
}
