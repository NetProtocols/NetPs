namespace NetPs.Tcp.Hole
{
    using System;
    using NetPs.Socket.Packets;

    public interface IHoleEvents
    {
        void OnReceivedPacket(HolePacket packet, TcpClient client);
    }
}
