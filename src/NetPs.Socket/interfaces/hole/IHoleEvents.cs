namespace NetPs.Socket.Hole
{
    using System;
    using NetPs.Socket.Packets;
    using System.Net;

    /// <summary>
    /// Hole 事件
    /// </summary>
    public interface IHoleEvents
    {
        /// <summary>
        /// 接收到Hole数据包
        /// </summary>
        /// <param name="packet">Hole包</param>
        /// <param name="addr">发送方地址</param>
        void OnReceivedPacket(HolePacket packet, IPEndPoint addr);
    }
}
