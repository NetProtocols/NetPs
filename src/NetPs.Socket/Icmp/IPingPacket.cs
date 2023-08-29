namespace NetPs.Socket.Icmp
{
    using System;
    using System.Net;

    public interface IPingPacket
    {
        public IPAddress Address { get; }
        byte Type { get; }
        byte Code { get; }
        ushort Checksum { get; }
        ushort Identifier { get; }
        ushort SequenceNumber { get; }
        byte[] Data { get; }

        byte[] GET();
    }
}
