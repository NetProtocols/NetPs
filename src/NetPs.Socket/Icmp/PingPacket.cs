namespace NetPs.Socket.Icmp
{
    using System;
    using System.Diagnostics;
    using System.Net;

    public enum PingPacketKind
    {
        UnKnown = 0,
        /// <summary>
        /// 请求
        /// </summary>
        Request = 1,
        /// <summary>
        /// 回显
        /// </summary>
        Response = 10,
    }

    public class PingPacket : IPingPacket
    {
        private byte[] x_data { get; set; }
        public IPAddress Address { get; set; }
        public byte Type { get; private set; }
        public byte Code { get; private set; }
        public ushort Checksum { get; private set; }
        public ushort Identifier { get; private set; }
        public ushort SequenceNumber { get; private set; }
        public byte[] Data { get; private set; }

        public PingPacket(byte[] data, PingPacketKind kind = PingPacketKind.Request)
        {
            switch (kind)
            {
                case PingPacketKind.Request: this.Type = 0b1000;
                    break;
                default: this.Type = 0b0;
                    break;
            }
            this.Checksum = 0;
            this.Identifier = this.GetCurrentProcessID();
            this.SequenceNumber = 1;
            this.Data = data;
        }

        public PingPacket(byte[] data)
        {
            Type = data[0];
            Code = data[1];
            Checksum = BitConverter.ToUInt16(new ReadOnlySpan<byte>(data, 2, 2).ToArray(), 0);
            Identifier = BitConverter.ToUInt16(new ReadOnlySpan<byte>(data, 4, 2).ToArray(), 0);
            SequenceNumber = BitConverter.ToUInt16(new ReadOnlySpan<byte>(data, 6, 2).ToArray(), 0);
            Data = new ReadOnlySpan<byte>(data, 8, data.Length - 8).ToArray();
        }

        public virtual PingPacket Next()
        {
            this.SequenceNumber++;
            return this;
        }

        public virtual byte[] GET()
        {
            var len = Data.Length + 8;
            if (x_data == null || x_data.Length != len) x_data = new byte[len];

            x_data[0] = this.Type;
            x_data[1] = this.Code;
            BitConverter.GetBytes(this.GetCurrentProcessID()).CopyTo(x_data, 4);
            BitConverter.GetBytes(this.SequenceNumber).CopyTo(x_data, 6);
            Data.CopyTo(x_data, 8);
            BitConverter.GetBytes(this.MakeCheckSum(x_data, len)).CopyTo(x_data, 2);
            return x_data;
        }

        public PingPacketKind Kind
        {
            get
            {
                if (Code != 0) return PingPacketKind.UnKnown;
                switch (Type)
                {
                    case 0b1000: return PingPacketKind.Request;
                    case 0b000: return PingPacketKind.Response;
                    default: return PingPacketKind.UnKnown;
                }
            }
        }


        public virtual ushort MakeCheckSum(byte[] packet_data, int size)
        {
            var x_out = 0;
            var counter = 0;
            packet_data[1] = 0;//checksum
            while (size > 0)
            {
                x_out += packet_data[counter];
                counter += 1;
                size -= 1;
            }

            x_out = (x_out >> 16) + (x_out & 0xffff);
            x_out += (x_out >> 16);
            return (ushort)(~x_out);
        }

        public virtual ushort GetCurrentProcessID()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                return (ushort)process.Id;
            }
            catch
            {
                return 45;
            }
        }
    }
}
