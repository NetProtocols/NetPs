namespace NetPs.Socket.Memory
{
    using System;
    using System.Collections.Generic;

    /// <remarks>
    /// 目的：方便 uint数组 和 byte数组 的交互
    /// </remarks>
    internal class uint_buf_reverse
    {
        internal struct ooo
        {
            internal uint[] Data { get; set; }
            internal ulong totalbytes { get; set; }
            internal uint used { get; set; }
            internal uint size { get; set; }
        }
        /// <summary>
        /// (O.o)
        /// </summary>
        internal ooo Oo;
        public IEnumerable<uint> Push(byte[] bytes, int offset, int length, int offset_last)
        {
            uint i = (uint)offset;
            byte x, y;

            do
            {
                x = (byte)(Oo.totalbytes & 0b11);
                if (x != 0)
                {
                    for (; x > 1; x--, i++)
                    {
                        Oo.Data[Oo.used] |= (uint)((bytes[i] << ((x)<< 3)));
                        if (length == i) break;
                    }
                    if (x != 0) break;
                    Oo.Data[Oo.used] |= (uint)(bytes[i] <<24);

                    if (x == 1)
                    {
                        Oo.used++;
                        if (Oo.used >= Oo.size - offset_last)
                        {
                            Oo.used = 0;
                            yield return i;
                        }
                    }
                    if (length == i)
                    {
                        break;
                    }
                }
                else if (Oo.totalbytes != 0 && Oo.used >= Oo.size - offset_last)
                {
                    Oo.used = 0;
                    yield return i;
                }
                Oo.totalbytes += (uint)length;

                for (; i + 3 < length;)
                {
                    Oo.Data[Oo.used] = (uint)((bytes[i]) | (bytes[i + 1] << 8) | (bytes[i + 2] << 16) | (bytes[i + 3] << 24));
                    Oo.used++;
                    i += 4;
                    if (Oo.used >= Oo.size - offset_last)
                    {
                        Oo.used = 0;
                        yield return i;
                    }
                }

                x = (byte)(Oo.totalbytes & 0b11);
                if (x != 0)
                {

                    Oo.Data[Oo.used] = 0;
                    for (y = 0; x > 0; x--, y++, i++)
                    {
                        Oo.Data[Oo.used] |= (uint)((bytes[i] << (y << 3)));
                        if (length == i) break;
                    }
                }
            } while (false);
        }
        public void PushNext(byte b)
        {
            if ((Oo.totalbytes & 0b11) != 0)
            {
                Oo.Data[Oo.used] |= (uint)(b << ((byte)(Oo.totalbytes & 0b11)<< 3));
            }
            else
            {
                Oo.Data[Oo.used] = (uint)b;
            }
            Oo.used++;
            if (Oo.used >= Oo.size)
            {
                Oo.used = 0;
            }
        }
        public void PushTotal()
        {
            Oo.Data[Oo.used++] = (uint)((Oo.totalbytes << 3) & 0xffffffff);
            Oo.Data[Oo.used++] = (uint)((Oo.totalbytes >> 29) & 0xffffffff);
            if (Oo.used >= Oo.size)
            {
                Oo.used = 0;
            }
        }
        public void Fill(uint x, int offset_last)
        {
            for(; Oo.used < Oo.size - offset_last; Oo.used++)
            {
                Oo.Data[Oo.used] = x;
            }
        }
        public void PushNext(uint x)
        {
            Oo.Data[Oo.used++] = x;
            if (Oo.used >= Oo.size)
            {
                Oo.used = 0;
            }
        }
        public bool IsFULL(int offset = 0)
        {
            // 最后一个
            if (Oo.used == 0) return Oo.totalbytes > 3;
            // 沾左边
            else
            {
                bool r;
                if (Oo.used + offset == Oo.size) r = Oo.totalbytes != 0;
                // 中间
                else r = Oo.used + offset > Oo.size;
                // 填充之后的数据, be已经满了!
                if (r) Fill(0, 0);
                return r;
            }
        }
        public static uint_buf_reverse New( uint size)
        {
            var buf = new ooo();
            buf.Data = new uint[size];
            buf.size = size;
            buf.totalbytes = 0;
            buf.used = 0;
            return new uint_buf_reverse { Oo = buf };
        }
    }
}
