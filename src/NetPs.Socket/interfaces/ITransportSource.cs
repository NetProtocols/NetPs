/**
 * 目标：发送多个相同内容又要用内存时，实现复用减少内存开销。
 * 
 * 01. 需要 "引用计数" 来得知活动状态，以方便后续的定时清理。
 * 02. 多个同时读取，每个的进度不同，这需要使用 偏移 + 读取长度 的方式进行读取
 * 03. 实现多种流：文件流、内存流。
 */
namespace NetPs.Socket
{
    using System;

    /// <summary>
    /// 发送内容源
    /// </summary>
    /// <remarks>
    /// 用于多次发送同一重复内容时，共用以节省内存开销。
    /// </remarks>
    public interface ITransportSource
    {
        /// <summary>
        /// 活动数量
        /// </summary>
        int LTimes { get; }
        /// <summary>
        /// 复制到内存缓冲区
        /// </summary>
        void CopyTo(byte[] buffer, int offset, int count);
        /// <summary>
        /// 添加发送任务
        /// </summary>
        void AddTask(IDataTransport transport);
        /// <summary>
        /// 删除发送任务
        /// </summary>
        void RemoveTask(IDataTransport transport);
        /// <summary>
        /// 发送任务是否存活
        /// </summary>
        bool IsAlive(IDataTransport transport);
    }
}
