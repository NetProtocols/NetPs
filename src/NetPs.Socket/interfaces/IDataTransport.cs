namespace NetPs.Socket
{
    using System;

    /// <summary>
    /// 数据传输基础
    /// </summary>
    public interface IDataTransport : IDisposable
    {
        /// <summary>
        /// 是否释放
        /// </summary>
        bool IsDisposed { get; }
        /// <summary>
        /// 运行状态
        /// </summary>
        bool Running { get; }
        /// <summary>
        /// 传输
        /// </summary>
        void Transport(byte[] data, int offset, int length);
        /// <summary>
        /// 要看结束事件
        /// </summary>
        void LookEndTransport(IEndTransport endTransport);
    }

    public static class IDataTransportExtra
    {
        public static void Transport(this IDataTransport tx, byte[] data) => tx.Transport(data, 0, -1);
    }
}
