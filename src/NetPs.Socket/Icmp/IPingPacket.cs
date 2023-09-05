namespace NetPs.Socket.Icmp
{
    using System;
    using System.Net;

    public interface IPingPacket
    {
        public IPAddress Address { get; }
        byte Type { get; }
        byte Code { get; }
        int Checksum { get; }
        int Identifier { get; }
        int SequenceNumber { get; }
        byte[] Data { get; }

        byte[] GET();
    }
}
