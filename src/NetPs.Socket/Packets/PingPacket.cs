﻿namespace NetPs.Socket.Packets
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

    public class PingPacket : IPingPacket, IPacket
    {
        public static readonly byte[] DATA_32 = { 0x2e, 0x2e, 0x2e, 0x2e, 0x20, 0x2e, 0x20, 0x2d, 0x2e, 0x2d, 0x2d, 0x20, 0x2e, 0x2d, 0x2d, 0x2e, 0x20, 0x2e, 0x2e, 0x20, 0x2d, 0x2e, 0x20, 0x2d, 0x2d, 0x2e, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
        private byte[] x_data { get; set; }
        public IPAddress Address { get; set; }
        public byte Type { get; private set; }
        public byte Code { get; private set; }
        public int Checksum { get; private set; }
        public int Identifier { get; private set; }
        public int SequenceNumber { get; private set; }
        public byte[] Data { get; private set; }
        public PingPacket(byte[] data, PingPacketKind kind = PingPacketKind.Request)
        {
            switch (kind)
            {
                case PingPacketKind.Request:
                    this.Type = 0b1000;
                    break;
                default:
                    this.Type = 0b0;
                    break;
            }
            this.Checksum = 0;
            this.Identifier = GetCurrentProcessID();
            this.SequenceNumber = 1;
            this.Data = data;
        }

        public PingPacket(byte[] data)
        {
            this.SetData(data);
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
            BitConverter.GetBytes(GetCurrentProcessID()).CopyTo(x_data, 4);
            BitConverter.GetBytes(this.SequenceNumber).CopyTo(x_data, 6);
            Data.CopyTo(x_data, 8);
            if (Address.IsIpv6())
            {
                x_data[0] <<= 4; //icmpv6: type 0x80, not ipv4 0x08
            }
            BitConverter.GetBytes((short)this.MakeCheckSum(x_data, len)).CopyTo(x_data, 2);
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


        public virtual int MakeCheckSum(byte[] packet_data, int size)
        {
            var x_out = 0;
            var counter = 0;
            packet_data[2] = 0;//checksum
            packet_data[3] = 0;//checksum
            while (--size > 0)
            {
                x_out += packet_data[counter++];
                x_out += packet_data[counter++] << 8;
                size --;
            }

            x_out = (x_out >> 16) + (x_out & 0xffff);
            x_out += (x_out >> 16);
            return (ushort)(~x_out);
        }

        public static int GetCurrentProcessID()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                return process.Id;
            }
            catch
            {
                return 45;
            }
        }

        public byte[] GetData()
        {
            return this.GET();
        }

        public void SetData(byte[] data, int offset)
        {
            Type = data[offset++];
            Code = data[offset++];
            Checksum = BitConverter.ToUInt16(new ReadOnlySpan<byte>(data, offset, 2).ToArray(), 0);
            offset += 2;
            Identifier = BitConverter.ToUInt16(new ReadOnlySpan<byte>(data, offset, 2).ToArray(), 0);
            offset += 2;
            SequenceNumber = BitConverter.ToUInt16(new ReadOnlySpan<byte>(data, offset, 2).ToArray(), 0);
            offset += 2;
            Data = new ReadOnlySpan<byte>(data, offset, data.Length - offset).ToArray();
        }

        public bool Verity(byte[] data, int offset)
        {
            if (data.Length <= 8) return false;
            return true;
        }
    }
}
