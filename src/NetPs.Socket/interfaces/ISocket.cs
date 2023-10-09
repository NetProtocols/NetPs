namespace NetPs.Socket
{
    using System;
    using System.Net;

    public interface ISocket
    {
        /// <summary>
        /// socket
        /// </summary>
        System.Net.Sockets.Socket Socket { get; }
        /// <summary>
        /// 当前地址.
        /// </summary>
        ISocketUri Address { get; }
        /// <summary>
        /// 终端地址.
        /// </summary>
        IPEndPoint IPEndPoint { get; }
        /// <summary>
        /// 远程地址
        /// </summary>
        ISocketUri RemoteAddress { get; }
        /// <summary>
        /// 远程地址
        /// </summary>
        IPEndPoint RemoteIPEndPoint { get; }
        /// <summary>
        /// 释放
        /// </summary>
        bool IsDisposed { get; }
        /// <summary>
        /// 关闭
        /// </summary>
        bool IsClosed { get; }
        /// <summary>
        /// 活动
        /// </summary>
        bool Actived { get; }
        /// <summary>
        /// 关闭
        /// </summary>
        bool IsShutdown { get; }
        /// <summary>
        /// 失去socket
        /// </summary>
        bool IsSocketClosed { get; }
        /// <summary>
        /// 引用状态
        /// </summary>
        bool IsReference { get; }

        void Close();
        void Lose();
        void WhenLoseConnected(ISocketLosed lose);
    }
}
