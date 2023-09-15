namespace NetPs.Socket.Hole
{
    using System;
    using NetPs.Socket.Packets;

    public interface IHoleServerEvents
    {
        void OnRegister(HolePacket packet);
        void OnStartHole(HolePacket packet);
        void OnHoled(HolePacket packet);
    }
}
