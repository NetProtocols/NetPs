namespace NetPs.Socket.Hole
{
    using NetPs.Socket.Packets;
    using System;
    using System.Net;

    public interface IHoleHostRecord
    {
        void Record(HolePacket packet);
        bool ContainsHoleId(string id);
        IPEndPoint FindAddr(string id);
    }
}
