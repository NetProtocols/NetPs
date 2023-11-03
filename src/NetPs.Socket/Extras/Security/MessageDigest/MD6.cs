namespace NetPs.Socket.Extras.Security.MessageDigest
{
    using NetPs.Socket.Memory;
    using System;

    ///<remarks>
    ///https://people.csail.mit.edu/rivest/pubs/RABCx08.pdf
    ///</remarks>
    public class MD6 : IHash
    {
        internal static readonly uint[] S0 = { 0x01234567, 0x89abcdef };
        internal static readonly uint[] Sm = { 0x7311c281, 0x2425cfa0 };
        internal static readonly uint[] Q =
        {
            0x7311c281, 0x2425cfa0, 0x64322864, 0x34aac8e7, 0xb60450e9, 0xef68b7c1,
            0xe8fb2390, 0x8d9f06f1, 0xdd2e76cb, 0xa691e5bf, 0x0cd0d63b, 0x2c30bc41,
            0x1f8ccf68, 0x23058f8a, 0x54e5ed5b, 0x88e3775d, 0x4ad12aae, 0x0a6d6031,
            0x3e7f16bb, 0x88222e0d, 0x8af8671d, 0x3fb50c2c, 0x995ad117, 0x8bd25c31,
            0xc878c1dd, 0x04c4b633, 0x3b72066c, 0x7a1552ac, 0x0d6f3522, 0x631effcb
        };
        internal const byte Q_len = 30;
        internal static MD6_CTX Init()
        {
            var c = new MD6_CTX();
            c.total = 0;
            c.used = 0;
            c.times = 0;
            c.size = 128;
            c.r = (40 + c.size / 4);
            if (c.r < MD6_RAW_SIZE)
            {
                c.r_buf = LoopArray<uint>.New(MD6_RAW_SIZE);
            }
            else
            {
                c.r_buf = LoopArray<uint>.New((int)c.r);
            }
            c.levels = 64;
            c.key = new uint[16];
            c.key_len = 0;

            c.buf = new byte[512];
            c.hash_buf = new uint[c.levels][];
            c.hash_ix = new byte[c.levels];
            c.hash_times = new uint[c.levels];
            return c;
        }
        internal const int MD6_BLOCK_SIZE = 512;
        internal const int MD6_KEY_SIZE = 128;
        internal const int HASH_LEN_SIZE = 32;
        internal const int MD6_CALC_SIZE = 128;
        internal const int MD6_RAW_SIZE = Q_len + 16 + 4 + MD6_CALC_SIZE;
        private static void X(ref uint a, ref uint b, LoopArray<uint> A, int pos)
        {
            a ^= A.Get(pos);
            b ^= A.Get(pos + 1);
        }
        private static void A(ref uint a, ref uint b, LoopArray<uint> A, int pos_a, int pos_b)
        {
            a ^= (A.Get(pos_a) & A.Get(pos_b));
            b ^= (A.Get(pos_a + 1) & A.Get(pos_b + 1));
        }
        private static void R(ref uint a, ref uint b, byte shift)
        {
            if (shift >= 32)
            {
                b ^= (a >>> (shift - 32));
                a ^= 0;
            }
            else
            {
                b ^= ((a << (32 - shift)) | (b >>> shift));
                a ^= (a >>> shift);
            }
        }
        private static void L(ref uint a, ref uint b, byte shift)
        {
            if (shift >= 32)
            {
                a ^= (b << (shift - 32));
                b ^= 0;
            }
            else
            {
                a ^= ((a << shift) | (b >>> (32 - shift)));
                b ^= (b << shift);
            }
        }
        internal static readonly byte[] MD6_T = { 34, 36, 42, 62, 134, 178 };
        internal static readonly byte[] MD6_RS = { 10, 5, 13, 10, 11, 12, 2, 7, 14, 15, 7, 13, 11, 7, 6, 12 };
        internal static readonly byte[] MD6_LS = { 11, 24, 9, 16, 15, 9, 27, 15, 6, 2, 29, 8, 15, 5, 31, 9 };
        private static void F(ref MD6_CTX c)
        {
            // block: 178 uint[]
            uint i, j, s, n;

            var s_0 = S0[0];
            var s_1 = S0[1];
            uint x_0, x_1;
            uint swap;
            for (j = 0,i = MD6_RAW_SIZE; j < (c.r << 1); j +=2, i += 32)
            {
                for(s = 0, n = 0; n < 16; n ++, s +=2)
                {
                    x_0 = s_0; x_1 = s_1;
                    X(ref x_0, ref x_1, c.r_buf, 0);
                    X(ref x_0, ref x_1, c.r_buf, 144);
                    A(ref x_0, ref x_1, c.r_buf, 142, 136);
                    A(ref x_0, ref x_1, c.r_buf, 116, 44);
                    R(ref x_0, ref x_1, MD6_RS[n]);
                    L(ref x_0, ref x_1, MD6_LS[n]);
                    c.r_buf.Push(x_0);
                    c.r_buf.Push(x_1);
                }

                swap = ((s_0 & Sm[0]) ^ ((s_0 << 1) | (s_1 >>> 31) ^ 0));
                s_1 = ((s_1 & Sm[1]) ^ ((s_1 << 1) ^ (s_0 >>> 31)));
                s_0 = swap;
            }
        }
        private static void RecordHash(ref MD6_CTX c, int levels = 0)
        {
            while (true)
            {
                int i = 1;
                if ((levels + 1) >= c.levels)
                {
                    i = 0;
                }
                if (c.hash_buf[levels] == null)
                {
                    c.hash_buf[levels] = new uint[MD6_CALC_SIZE];
                    if (i == 0) c.hash_ix[levels] = HASH_LEN_SIZE;
                }
                c.r_buf.CopyTo(178 - HASH_LEN_SIZE, c.hash_buf[levels], c.hash_ix[levels], HASH_LEN_SIZE);
                if (c.hash_ix[levels] == 128 - HASH_LEN_SIZE)
                {
                    PrepareRbuf(ref c, 0, 0, c.hash_times[levels], (uint)(levels + 1));
                    c.r_buf.Push(c.hash_buf[levels], 0, MD6_CALC_SIZE);
                    F(ref c);

                    c.hash_ix[levels] = 0;
                    c.hash_times[levels]++;
                    levels += i;
                    continue;
                }
                else
                {
                    c.hash_ix[levels] += HASH_LEN_SIZE;
                }
                break;
            }
        }
        private static uint CalcP(long len)
        {
            uint p = (uint)((MD6_BLOCK_SIZE - len % MD6_BLOCK_SIZE) << 3);
            if (p == (MD6_BLOCK_SIZE << 3)) p = 0;
            return p;
        }
        private static void PrepareRbuf(ref MD6_CTX c, uint p, uint z, uint i, uint deep)
        {
            c.r_buf.Push(Q, 0, Q_len);
            c.r_buf.Push(c.key, 0, 16);
            c.r_buf.Push((((deep + 1) & 0xff) << 24) | ((uint)(i / Math.Pow(2, 32)) & 0xffffff));
            c.r_buf.Push(i & 0xffffffff);
            c.r_buf.Push(((c.r & 0xfff) << 16) | ((c.levels & 0xff) << 8) | ((z & 0xf) << 4) | ((p & 0xf000) >> 12));
            c.r_buf.Push(((p & 0xfff) << 20) | ((c.key_len & 0xff) << 12) | (c.size & 0xfff));
        }
        private static void Mid(ref MD6_CTX c, byte[] data, uint pos, int length)
        {
            uint j, h,raw;
            for (j = 0, h = pos; j < length>>2; j++, h+=4)
            {
                c.r_buf.Push((uint)((data[h] << 24) | (data[h + 1] << 16) | (data[h + 2] << 8) | data[h + 3]));
            }
            if ((length & 0b11) != 0)
            {
                raw = (uint)(data[h] << 24);
                if ((length & 0b10) != 0)
                {
                    raw |= (uint)(data[h+1] << 16);
                    if ((length & 0b1) != 0)
                    {
                        raw |= (uint)(data[h+2] << 8);
                    }
                }
                c.r_buf.Push(raw);
                h += 4;
            }
            if (MD6_CALC_SIZE > j) c.r_buf.Push(0, (int)(MD6_CALC_SIZE - (h >> 2)));
            F(ref c);
        }
        internal static void Update(ref MD6_CTX c, byte[] data, int len)
        {
            uint j;

            c.total += len;
            len += c.used;

            for(j = 0; j < len; j+= MD6_BLOCK_SIZE)
            {
                if (len - j <= MD6_BLOCK_SIZE)
                {
                    Array.Copy(data, (int)j, c.buf, c.used, (int)(len -j));
                    c.used += (int)(len - j);
                }
                else
                {
                    PrepareRbuf(ref c, 0, 0, c.times++, 0);
                    if (c.used > 0)
                    {
                        Array.Copy(data, 0, c.buf, c.used, MD6_BLOCK_SIZE - c.used);
                        j = 0;
                        len -= MD6_BLOCK_SIZE;
                        c.used = 0;
                        Mid(ref c, c.buf, j, MD6_BLOCK_SIZE);
                    }
                    else
                    {
                        Mid(ref c, data, j, MD6_BLOCK_SIZE);
                    }

                    RecordHash(ref c);
                }
            }
        }

        internal static byte[] Final(ref MD6_CTX c)
        {
            var len = c.total;
            uint p = CalcP(len);
            uint z = 1;
            if (len > MD6_BLOCK_SIZE) z = 0;
            PrepareRbuf(ref c, p, z, c.times, 0);
            Mid(ref c, c.buf, 0, c.used);
            var hash = new byte[c.size >> 3];
            var levels = 0;
            while (true)
            {
                if (c.hash_ix[levels] == 0)
                {
                    if ((levels + 1) >= c.levels || c.hash_buf[levels + 1] == null)
                    {
                        int j, k;
                        for (k = 0, j = (int)(MD6_RAW_SIZE - (c.size >> 5)); k < (int)(c.size >> 5); k++, j++)
                        {
                            hash.CopyFrom_Reverse(c.r_buf.Get(j), k << 2);
                        }
                        break;
                    }
                }
                c.r_buf.CopyTo(MD6_RAW_SIZE - HASH_LEN_SIZE, c.hash_buf[levels], c.hash_ix[levels], HASH_LEN_SIZE);
                len = ((c.hash_times[levels]) << 9) + ((c.hash_ix[levels] + HASH_LEN_SIZE) << 2);
                p = CalcP(len);

                var i = 1;
                if ((levels + 1) >= c.levels)
                {
                    i = 0;
                    z = 1;
                }
                else
                {
                    z = 1;
                    if (len > MD6_BLOCK_SIZE) z = 0;
                }

                PrepareRbuf(ref c, p, z, c.hash_times[levels], (uint)(levels + 1));
                c.r_buf.Push(c.hash_buf[levels], 0, c.hash_ix[levels] + HASH_LEN_SIZE);
                if (c.hash_ix[levels] != MD6_CALC_SIZE - HASH_LEN_SIZE)
                {
                    c.r_buf.Push(0, MD6_CALC_SIZE - HASH_LEN_SIZE - c.hash_ix[levels]);
                }
                F(ref c);

                c.hash_ix[levels] = 0;
                levels += i;
            }

            return hash;
        }
        public string Make(byte[] data)
        {
            var len = data.Length;
            var c = Init();
            Update(ref c, data, len);
            return Final(ref c).ToHexString();
        }
    }

    public class MD6_512 : IHash
    {
        public string Make(byte[] data)
        {
            var c = MD6.Init();
            c.SetSize(512);
            MD6.Update(ref c, data, data.Length);
            return MD6.Final(ref c).ToHexString();
        }
    }
    public class MD6_256 : IHash
    {
        public string Make(byte[] data)
        {
            var c = MD6.Init();
            c.SetSize(256);
            MD6.Update(ref c, data, data.Length);
            return MD6.Final(ref c).ToHexString();
        }
    }
    public class MD6_128 : IHash
    {
        public string Make(byte[] data)
        {
            var c = MD6.Init();
            c.SetSize(128);
            MD6.Update(ref c, data, data.Length);
            return MD6.Final(ref c).ToHexString();
        }
    }
}
