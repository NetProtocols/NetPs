namespace NetPs.Udp.Hole
{
    using NetPs.Socket.Packets;
    using System;

    public interface IHoleEvents
    {
        void OnReceivedPacket(HolePacket packet);
    }
}
