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
        bool Verity(byte[] data, int offset);
    }

    public static class IPacketExtra
    {
        public static void SetData(this IPacket packet, byte[] data) => packet.SetData(data, 0);
        public static bool Verity(this IPacket packet, byte[] data) => packet.Verity(data, 0);
    }
}
