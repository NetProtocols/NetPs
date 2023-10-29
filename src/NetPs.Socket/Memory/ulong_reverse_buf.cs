namespace NetPs.Socket.Memory
{
    using System;
    using System.Collections.Generic;

    /// <remarks>
    /// 目的：方便 ulong 和 byte数组 的交互
    /// </remarks>
    internal class ulong_reverse_buf
    {
        internal struct ooo
        {
            internal ulong[] Data { get; set; }
            internal ulong totalbytes_high { get; set; }
            internal ulong totalbytes_low { get; set; }
            internal uint used { get; set; }
            internal uint size { get; set; }
        }
        /// <summary>
        /// (O.o)
        /// </summary>
        internal ooo Oo;
        public void SetByte(byte value, int position)
        {
            Oo.Data[position / 8] |= (ulong)value << (byte)(((position & 0b111)) << 3);
        }
        public void ToNext()
        {
            Oo.used++;
        }
        public IEnumerable<uint> Push(byte[] bytes, int offset, int length, int offset_last)
        {
            uint i = (uint)offset;
            ulong temp;
            byte x, y;

            do
            {
                x = (byte)(Oo.totalbytes_low & 0b111);
                if (x != 0)
                {
                    for (; x > 1; x--, i++)
                    {
                        Oo.Data[Oo.used] |= (ulong)bytes[i] << ((x) << 3);
                        if (length == i) break;
                    }
                    if (x != 0) break;
                    Oo.Data[Oo.used] |= (ulong)bytes[i] << 56;

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
                else if (Oo.totalbytes_low != 0 && Oo.totalbytes_high != 0 && Oo.used >= Oo.size - offset_last)
                {
                    Oo.used = 0;
                    yield return i;
                }
                temp = (Oo.totalbytes_low + (ulong)length) & 0xffffffffffffffff;
                if (temp < Oo.totalbytes_low)
                {
                    Oo.totalbytes_high++;
                }
                Oo.totalbytes_low = temp;

                for (; i + 7 < length;)
                {
                    Oo.Data[Oo.used] = ((ulong)bytes[i]) | ((ulong)bytes[i + 1] << 8) | ((ulong)bytes[i + 2] << 16) | ((ulong)bytes[i + 3]<<24) | ((ulong)bytes[i + 4] << 32) | ((ulong)bytes[i + 5] << 40) | ((ulong)bytes[i + 6] << 48) | ((ulong)bytes[i + 7] << 56) ;
                    Oo.used++;
                    i += 8;
                    if (Oo.used >= Oo.size - offset_last)
                    {
                        Oo.used = 0;
                        yield return i;
                    }
                }

                x = (byte)(Oo.totalbytes_low & 0b111);
                if (x != 0)
                {
                    Oo.Data[Oo.used] = 0;
                    for (y = 0; x > 0; x--, y++, i++)
                    {
                        Oo.Data[Oo.used] |= (ulong)bytes[i] << (y << 3);
                        if (length == i) break;
                    }
                }
            } while (false);
        }
        public void PushNext(byte b)
        {
            if ((Oo.totalbytes_low & 0b111) != 0)
            {
                Oo.Data[Oo.used] |= (ulong)b << ((byte)(7 - Oo.totalbytes_low & 0b111)<< 3);
            }
            else
            {
                Oo.Data[Oo.used] = (ulong)b << 56;
            }
            Oo.used++;
            if (Oo.used >= Oo.size)
            {
                Oo.used = 0;
            }
        }
        public void PushTotal()
        {
            Oo.Data[Oo.used++] = (Oo.totalbytes_high << 3)  | ((Oo.totalbytes_low & 0xfff0000000000000)>>52);
            Oo.Data[Oo.used++] = Oo.totalbytes_low << 3;
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
        public bool NotFirstFull => Oo.totalbytes_high > 0 && Oo.totalbytes_low > 3 && Oo.used == 0;
        public uint Used => Oo.used;
        public int UsedBytes => (int)(Oo.used<<3) + (byte)(Oo.totalbytes_low % 8);
        public static ulong_reverse_buf New(uint size)
        {
            var buf = new ooo();
            buf.Data = new ulong[size];
            buf.size = size;
            buf.totalbytes_low = 0;
            buf.totalbytes_high = 0;
            buf.used = 0;
            return new ulong_reverse_buf { Oo = buf };
        }
    }
}
