namespace NetPs.Socket.Packets
{
    using System;
    using NetPs.Socket;
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
        Fuzhu = 0b00001000,//辅助
        FuzhuCallback = 0b00001001,
        FuzhuCallbackError = 0b00001011,
        Hole = 0b00010000,//打洞
        HoleCallback = 0b00010001,
        HoleCallbackError = 0b00010011,
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
        FuzhuTag = 0b00100000,
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
        /// 错误标识
        /// </summary>
        public bool HasError { get; set; }
        /// <summary>
        /// 回传标识
        /// </summary>
        public bool IsCallback { get; set; }
        /// <summary>
        /// 标识
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 密钥
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public IPEndPoint Address { get; set; }
        /// <summary>
        /// 辅助标识
        /// </summary>
        public string FuzhuTag { get; set; }
        /// <summary>
        /// 辅助地址
        /// </summary>
        public IPAddress FuzhuAddress { get; set; }
        /// <summary>
        /// 辅助端口清单
        /// </summary>
        public int[] FuzhuPorts { get; set; }

        public IPEndPoint Source { get; set; }
        public HolePacket(HolePacketOperation operation, string Id, string Key)
        {
            this.Operation = operation;
            this.Id = Id;
            this.Key = Key;
        }
        public HolePacket()
        {
        }
        public virtual int Read(byte[] buffer, int offset = 0)
        {
            using (var queue = new QueueStream(buffer, offset))
            {
                var b = queue.DequeueByte();
                if (CheckBit(b, 3))
                {
                    Operation = HolePacketOperation.Register;
                }
                else if (CheckBit(b, 4))
                {
                    Operation = HolePacketOperation.Fuzhu;
                }
                else if (CheckBit(b, 5))
                {
                    Operation = HolePacketOperation.Hole;
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
                    else if (CheckBit(b, 6))
                    {
                        //fuzhu tag
                        var len = queue.DequeueUInt16();
                        len = queue.RequestRead(len);

                        this.FuzhuTag = Encoding.ASCII.GetString(queue.Buffer, (int)queue.ReadPosition, len);
                        queue.RecordRead(len);

                        b = queue.DequeueByte();
                        if (CheckBit(b, 3))
                        {
                            if (CheckBit(b, 2))
                            {
                                //ipv6
                                this.FuzhuAddress = new IPAddress(queue.Dequeue(8));
                            }
                            else
                            {
                                //ipv4
                                this.FuzhuAddress = new IPAddress(queue.Dequeue(4));
                            }

                            var count = (int)queue.DequeueByte();
                            this.FuzhuPorts = new int[count];
                            while(count-- > 0)
                            {
                                this.FuzhuPorts[count] = queue.DequeueUInt16();
                            }
                        }
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

        public virtual byte[] GetData()
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

                if (Operation == HolePacketOperation.Fuzhu)
                {
                    if (this.FuzhuTag != null)
                    {
                        queue.EnqueueByte((byte)HolePacketTag.FuzhuTag);
                        var buf = Encoding.ASCII.GetBytes(this.FuzhuTag);
                        queue.EnqueueUInt16(buf.Length);
                        queue.EnqueueByte(buf);
                        if (this.FuzhuAddress != null)
                        {
                            var ip = this.FuzhuAddress.GetAddressBytes();
                            if (ip.Length == 4)
                            {
                                queue.EnqueueByte((byte)HolePacketTag.AddressV4);
                                queue.Enqueue(ip);
                            }
                            else if (ip.Length == 8)
                            {
                                queue.EnqueueByte((byte)HolePacketTag.AddressV6);
                                queue.Enqueue(ip);
                            }

                            var count = (byte)FuzhuPorts.Length;
                            queue.EnqueueByte(count);
                            while (count-- > 0)
                            {
                                queue.EnqueueUInt16(FuzhuPorts[count]);
                            }
                        }
                        else
                        {
                            queue.EnqueueByte((byte)HolePacketTag.End);
                        }
                    }
                }

                queue.EnqueueByte((byte)HolePacketTag.End);
                return queue.Dequeue(queue.Length);
            }
        }

        public virtual void Fuzhu(string tag, IPAddress address, params int[] ports)
        {
            Fuzhu(tag);
            this.FuzhuAddress = address;
            this.FuzhuPorts = ports;
        }
        public virtual void Fuzhu(string tag)
        {
            this.FuzhuTag = tag;
            this.Operation = HolePacketOperation.Fuzhu;
        }
        private bool CheckBit(byte b, int no)
        {
            return ((b >> (no -1)) & 1) == 1;
        }

        public void SetData(byte[] data, int offset)
        {
            if (!this.Verity(data, offset))
            {
                this.HasError = true;
                return;
            }
            this.Read(data, offset);
        }

        public bool Verity(byte[] data, int offset)
        {
            if (data.Length - offset <= 1) return false;
            var b = data[offset];
            if ((b & 0b11000000) != 0) return false;
            if (b == 0b10 || b == 0b11 || b == 0b0) return false;
            while (true)
            {
                if (data.Length <= ++offset) return false;
                b = data[offset];
                switch (b)
                {
                    case (byte)HolePacketTag.ID:
                    case (byte)HolePacketTag.Key:
                        if (data.Length <= ++offset +1) return false;
                        b = data[offset++];
                        offset += b + (ushort)(data[offset] << 8);
                        continue;
                    case (byte)HolePacketTag.FuzhuTag:
                        if (data.Length <= ++offset + 1) return false;
                        b = data[offset++];
                        offset += b + (ushort)(data[offset] << 8);
                        if (data.Length <= ++offset) return false;
                        b = data[offset];
                        switch (b)
                        {
                            case (byte)HolePacketTag.AddressV4:
                                offset += 4;
                                break;
                            case (byte)HolePacketTag.AddressV6:
                                offset += 8;
                                break;
                            case (byte)HolePacketTag.End: continue;
                            default: return false;
                        }
                        if (data.Length <= ++offset) return false;
                        b = data[offset];
                        offset += b * 2;
                        continue;
                    case (byte)HolePacketTag.AddressV4:
                        offset += 6;
                        continue;
                    case (byte)HolePacketTag.AddressV6:
                        offset += 10;
                        continue;
                    case (byte)HolePacketTag.End: break;
                    default: return false;
                }
                break;
            }

            return true;
        }
    }
}
