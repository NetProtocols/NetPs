namespace NetPs.Socket.Memory
{
    using System;
    using System.Collections.Generic;

    /// <remarks>
    /// 目的：方便 uint数组 和 byte数组 的交互
    /// </remarks>
    internal class uint_buf
    {
        public uint[] Data { get; set; }
        internal ulong totalbytes { get; private set; }
        internal uint used { get; private set; }
        internal uint size { get; private set; }
        public IEnumerable<uint> Push(byte[] bytes, int offset, int length, int offset_last)
        {
            uint i = (uint)offset;
            byte x;

            do
            {
                x = (byte)(totalbytes & 0b11);
                if (x != 0)
                {
                    for (; x > 1; x--, i++)
                    {
                        Data[used] |= (uint)((bytes[i] << ((3 - x)<< 3)));
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
                else if (totalbytes != 0 && used >= size - offset_last)
                {
                    used = 0;
                    yield return i;
                }
                totalbytes += (uint)length;

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

                x = (byte)(totalbytes & 0b11);
                if (x != 0)
                {

                    Data[used] = (uint)((bytes[i] << 24));
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
            if ((totalbytes & 0b11) != 0)
            {
                Data[used] |= (uint)(b << ((byte)(3 - totalbytes & 0b11)<< 3));
            }
            else
            {
                Data[used] = (uint)b << 24;
            }
            used++;
            if (used >= size)
            {
                used = 0;
            }
        }
        public void PushTotal()
        {
            Data[used++] = (uint)((totalbytes >> 29) & 0xffffffff);
            Data[used++] = (uint)((totalbytes << 3) & 0xffffffff);
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
        public bool NotFirstFull => totalbytes > 3 && used == 0;
        public static uint_buf New(uint size)
        {
            var buf = new uint_buf();
            buf.Data = new uint[size];
            buf.size = size;
            buf.totalbytes = 0;
            buf.used = 0;
            return buf;
        }
    }
}
