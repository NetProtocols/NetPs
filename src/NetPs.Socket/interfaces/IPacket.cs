namespace NetPs.Socket
{
    using System;

    public interface IPacket
    {
        /// <summary>
        /// 获取数据
        /// </summary>
        byte[] GetData();
        /// <summary>
        /// 放置数据
        /// </summary>
        void SetData(byte[] data, int offset);
        /// <summary>
        /// 校验
        /// </summary>
        /// <remarks>校验数据包可读性。</remarks>
        bool Verity(byte[] data, int offset);
    }

    public static class IPacketExtra
    {
        /// <summary>
        /// 放置数据
        /// </summary>
        /// <remarks>
        /// 放置全部数据。offset: 0
        /// </remarks>
        public static void SetData(this IPacket packet, byte[] data) => packet.SetData(data, 0);

        /// <summary>
        /// 校验
        /// </summary>
        /// <remarks>
        /// 校验全部数据。offset: 0
        /// </remarks>
        public static bool Verity(this IPacket packet, byte[] data) => packet.Verity(data, 0);
    }
}
