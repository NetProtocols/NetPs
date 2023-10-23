namespace NetPs.Socket.Memory
{
    using System;
    using System.Collections.Generic;

    /// <remarks>
    /// 目的：方便 ulong 和 byte数组 的交互
    /// </remarks>
    internal class ulong_buf
    {
        public ulong[] Data { get; set; }
        internal ulong totalbytes_high { get; private set; }
        internal ulong totalbytes_low { get; private set; }
        internal uint used { get; private set; }
        internal uint size { get; private set; }
        public IEnumerable<uint> Push(byte[] bytes, int offset, int length, int offset_last)
        {
            uint i = (uint)offset;
            ulong temp;
            byte x;

            do
            {
                x = (byte)(totalbytes_low & 0b111);
                if (x != 0)
                {
                    for (; x > 1; x--, i++)
                    {
                        Data[used] |= (uint)((bytes[i] << ((7 - x) << 3)));
                        if (length == i) break;
                    }
                    if (x != 0) break;
                    Data[used] |= (uint)(bytes[i]);

                    if (x == 1)
                    {
                        used++;
                        if (used >= size - offset_last)
                        {
                            used = 0;
                            yield return i;
                        }
                    }
                    if (length == i)
                    {
                        break;
                    }
                }
                else if (totalbytes_low != 0 && totalbytes_high != 0 && used >= size - offset_last)
                {
                    used = 0;
                    yield return i;
                }
                temp = (totalbytes_low + (ulong)length) & 0xffffffffffffffff;
                if (temp < totalbytes_low)
                {
                    totalbytes_high++;
                }
                totalbytes_low = temp;

                for (; i + 3 < length;)
                {
                    Data[used] = (uint)((bytes[i] << 24) | (bytes[i + 1] << 16) | (bytes[i + 2] << 8) | (bytes[i + 3]));
                    used++;
                    i += 4;
                    if (used >= size - offset_last)
                    {
                        used = 0;
                        yield return i;
                    }
                }

                x = (byte)(totalbytes_low & 0b111);
                if (x != 0)
                {
                    Data[used] = (uint)((bytes[i] << 56));
                    for (; x > 0; x--, i++)
                    {
                        Data[used] |= (uint)((bytes[i] << (x << 3)));
                        if (length == i) break;
                    }
                }
            } while (false);
        }
        public void PushNext(byte b)
        {
            if ((totalbytes_low & 0b111) != 0)
            {
                Data[used] |= (uint)(b << ((byte)(7 - totalbytes_low & 0b111)<< 3));
            }
            else
            {
                Data[used] = (uint)b << 56;
            }
            used++;
            if (used >= size)
            {
                used = 0;
            }
        }
        public void PushTotal()
        {
            Data[used++] = totalbytes_high;
            Data[used++] = totalbytes_low;
            if (used >= size)
            {
                used = 0;
            }
        }
        public void Fill(uint x, int offset_last)
        {
            for(;used < size - offset_last; used++)
            {
                Data[used] = x;
            }
        }
        public void PushNext(uint x)
        {
            Data[used++] = x;
            if (used >= size)
            {
                used = 0;
            }
        }
        public bool NotFirstFull => totalbytes_high > 0 && totalbytes_low > 3 && used == 0;
        public static ulong_buf New(uint size)
        {
            var buf = new ulong_buf();
            buf.Data = new ulong[size];
            buf.size = size;
            buf.totalbytes_low = 0;
            buf.totalbytes_high = 0;
            buf.used = 0;
            return buf;
        }
    }
}
