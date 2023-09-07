namespace NetPs.Tcp.Hole
{
    using NetPs.Socket;
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    /// <summary>
    /// 操作类型识别
    /// </summary>
    public enum HolePacketOperation : byte
    {
        UnKnown = 0, //未知
        Register = 0b00000100, // 注册
        RegisterCallback = 0b00000101,
        RegisterCallbackError = 0b00000111,
        GetId = 0b00010000,//获取
        GetIdCallback = 0b00010001,
        GetIdCallbackError = 0b00010011,
        HoleReady = 0b00011001, //准备完成
        CheckId = 0b00100000,//认证
        CheckIdCallback = 0b00100001,
        CheckIdCallbackError = 0b00100011,
    }

    /// <summary>
    /// 标签类型识别
    /// </summary>
    public enum HolePacketTag : byte
    {
        AddressV4 = 0b00000100,
        AddressV6 = 0b00000110,
        Key = 0b00001000,
        ID = 0b00010000,
        End = 0b10000000,
    }

    /// <summary>
    /// hole 数据包
    /// </summary>
    /// <remarks>
    /// <br/> 0~8bit:   操作码
    /// <br/> 0~8bit:   标识
    /// <br/> 0~16 bit: Id 长度
    /// <br/>           string Id
    /// <br/> 0~8bit:   标识
    /// <br/> 0~16 bit: Key 长度
    /// <br/>           string Key
    /// </remarks>
    public class HolePacket : IPacket
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public HolePacketOperation Operation { get; set; }
        /// <summary>
        /// 是否发生错误
        /// </summary>
        public bool HasError { get; set; }
        public bool IsCallback { get; set; }

        public string Id { get; set; }
        public string Key { get; set; }
        public IPEndPoint Address { get; set; }

        public HolePacket(HolePacketOperation operation, string Id, string Key)
        {
            this.Operation = operation;
            this.Id = Id;
            this.Key = Key;
        }
        public HolePacket()
        {
        }
        public int Read(byte[] buffer, int offset = 0)
        {
            using (var queue = new QueueStream(buffer, offset))
            {
                var b = queue.DequeueByte();
                if (CheckBit(b, 3))
                {
                    Operation = HolePacketOperation.Register;
                }
                else if (CheckBit(b, 5))
                {
                    Operation = HolePacketOperation.GetId;
                }
                else if (CheckBit(b, 6))
                {
                    Operation = HolePacketOperation.CheckId;
                }
                else
                {
                    Operation = HolePacketOperation.UnKnown;
                    return -1;
                }

                IsCallback = CheckBit(b, 1);
                HasError = CheckBit(b, 2);

                while (!queue.IsEmpty)
                {
                    b = queue.DequeueByte();
                    if (CheckBit(b, 3))
                    {
                        if (CheckBit(b, 2))
                        {
                            //ipv6
                            var ip = new IPAddress(queue.Dequeue(8));
                            var port = queue.DequeueUInt16();
                            this.Address = new IPEndPoint(ip, port);
                        }
                        else
                        {
                            //ipv4
                            var ip = new IPAddress(queue.Dequeue(4));
                            var port = queue.DequeueUInt16();
                            this.Address = new IPEndPoint(ip, port);
                        }
                    }
                    else if (CheckBit(b, 4))
                    {
                        //key
                        var len = queue.DequeueUInt16();
                        len = queue.RequestRead(len);

                        this.Key = Encoding.ASCII.GetString(queue.Buffer, (int)queue.ReadPosition , len);
                        queue.RecordRead(len);

                    }
                    else if (CheckBit(b, 5))
                    {
                        //id
                        var len = queue.DequeueUInt16();
                        len = queue.RequestRead(len);

                        this.Id = Encoding.ASCII.GetString(queue.Buffer, (int)queue.ReadPosition, len);
                        queue.RecordRead(len);
                    }
                    else if (CheckBit(b, 8))
                    {
                        //end
                        break;
                    }
                }
                return (int)queue.Length;
            }
        }

        public byte[] GetData()
        {
            using (var queue = new QueueStream())
            {
                var b = (byte)Operation;
                if (this.HasError) b |= 0b10;
                if (this.IsCallback) b |= 0b1;

                queue.EnqueueByte(b);
                if (this.Id != null)
                {
                    queue.EnqueueByte((byte)HolePacketTag.ID);
                    var buf = Encoding.ASCII.GetBytes(this.Id);
                    queue.EnqueueUInt16(buf.Length);
                    queue.EnqueueByte(buf);
                }
                if (this.Key != null)
                {
                    queue.EnqueueByte((byte)HolePacketTag.Key);
                    var buf = Encoding.ASCII.GetBytes(this.Key);
                    queue.EnqueueUInt16(buf.Length);
                    queue.EnqueueByte(buf);
                }
                if (this.Address != null)
                {
                    var ip = this.Address.Address.GetAddressBytes();
                    var port = this.Address.Port;
                    if (ip.Length == 4)
                    {
                        queue.EnqueueByte((byte)HolePacketTag.AddressV4);
                        queue.Enqueue(ip);
                        queue.EnqueueUInt16(port);
                    }
                    else if (ip.Length == 8)
                    {
                        queue.EnqueueByte((byte)HolePacketTag.AddressV6);
                        queue.Enqueue(ip);
                        queue.EnqueueUInt16(port);
                    }

                }

                queue.EnqueueByte((byte)HolePacketTag.End);
                return queue.Dequeue(queue.Length);
            }
        }

        private bool CheckBit(byte b, int no)
        {
            return ((b >> (no -1)) & 1) == 1;
        }
    }
}
