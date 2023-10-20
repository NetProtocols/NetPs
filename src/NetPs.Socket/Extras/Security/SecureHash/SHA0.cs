namespace NetPs.Socket.Extras.Security.SecureHash
{
    using System;
    /// <summary>
    /// https://nvlpubs.nist.gov/nistpubs/Legacy/FIPS/NIST.FIPS.180.pdf
    /// </summary>
    public class SHA0
    {
        internal const uint BLOCK_SIZE = 512;
        internal const uint WORD_SIZE = BLOCK_SIZE - 64;
        internal static uint F1(uint b, uint c, uint d)
        {
            return (b & c) | ((~b) & d);
        }
        internal static uint F2(uint b, uint c, uint d)
        {
            return b ^ c ^ d;
        }
        internal static uint F3(uint b, uint c, uint d)
        {
            return (b & c) | (b & d) | (c & d);
        }
        internal delegate uint Func(uint b, uint c, uint d);
        internal static uint S(uint x, byte shift)
        {
            return (x << shift) | (x >> (32 - shift));
        }
        internal static SHA0_CTX Init()
        {
            var c = new SHA0_CTX();
            c.buf = new uint[BLOCK_SIZE >> 5];
            c.a = 0x67452301;
            c.b = 0xEFCDAB89;
            c.c = 0x98BADCFE;
            c.d = 0x10325476;
            c.e = 0xC3D2E1F0;
            c.total = 0;
            c.used = 0;
            return c;
        }
        internal static readonly uint[] K = { 0x5A827999, 0x6ED9EBA1, 0x8F1BBCDC, 0xCA62C1D6 };
        internal static readonly Func[] F = { F1, F2, F3, F2 }; 
        internal static void ProcessBlock(ref SHA0_CTX ctx)
        {
            uint i,s, temp;
            uint a, b, c, d, e;
            a = ctx.a;
            b = ctx.b;
            c = ctx.c;
            d = ctx.d;
            e = ctx.e;
            for (i = 0; i < 80; i++)
            {
                s = i & 0xf;
                if (i >= 16)
                {
                    ctx.buf[s] = ctx.buf[(s + 13) & 0xf] ^ ctx.buf[(s + 8) & 0xf] ^ ctx.buf[(s + 2) & 0xf] ^ ctx.buf[s];
                }
                temp = S(a, 5) + F[i/20](b, c, d) + e + ctx.buf[s]  + K[i/20];
                e = d;
                d = c;
                c = S(b, 30);
                b = a;
                a = temp;
            }
            ctx.a += a;
            ctx.b += b;
            ctx.c += c;
            ctx.d += d;
            ctx.e += e;
        }
        internal static void Update(ref SHA0_CTX c, byte[] data, int length)
        {
            uint i = 0, j = c.used;
            if (j > 0)
            {
                if ((c.total & 0b11) != 0)
                {
                    if ((c.total & 0b10) != 0)
                    {
                        if ((c.total & 0b1) == 0)
                        {
                            c.buf[j] |= (uint)(data[i++] << 16);
                            if (length == i) return;
                        }
                        c.buf[j] |= (uint)(data[i++] << 8);
                        if (length == i) return;
                    }
                    c.buf[j] |= (data[i++]);
                    c.used++;
                    if (length == i) return;
                }
                j++;
            }

            c.total += (uint)length;

            for (; i + 3 < length; i += 4)
            {
                c.buf[j] = (uint)((data[i] << 24) | (data[i + 1] << 16) | (data[i + 2] << 8) | (data[i + 3]));
                j++;
                if (j == BLOCK_SIZE >> 5)
                {
                    ProcessBlock(ref c);
                    j = 0;
                }
            }

            c.used = j;
            if ((c.total & 0b11) != 0)
            {
                c.buf[j] = (uint)(data[i++] << 24);
                if (length == i) return;
                if ((c.total & 0b10) != 0)
                {
                    c.buf[j] |= (uint)(data[i++] << 16);
                    if (length == i) return;
                    if ((c.total & 0b1) != 0)
                    {
                        c.buf[j] |= (uint)(data[i++] << 8);
                        if (length == i) return;
                    }
                }
            }
        }
        internal static byte[] Final(ref SHA0_CTX c)
        {
            uint i = c.used;
            if ((c.total & 0b11) != 0)
            {
                c.buf[i] |= (uint)(1 << ((byte)(4 - c.total & 0b11) << 3) - 1);
            }
            else
            {
                c.buf[i] = (uint)1 << 31;
            }
            i++;
            if (i +1 >= BLOCK_SIZE >> 5)
            {
                ProcessBlock(ref c);
                i = 0;
            }

            for (; i < WORD_SIZE >> 5; i++)
            {
                c.buf[i] = 0;
            }

            c.buf[i] = (uint)((c.total >> 29) & 0xffffffff);
            c.buf[i + 1] = (uint)((c.total << 3) & 0xffffffff);

            ProcessBlock(ref c);

            byte[] sha = new byte[20];
            sha.CopyFrom_Reverse(c.a, 0);
            sha.CopyFrom_Reverse(c.b, 4);
            sha.CopyFrom_Reverse(c.c, 8);
            sha.CopyFrom_Reverse(c.d, 12);
            sha.CopyFrom_Reverse(c.e, 16);
            return sha;
        }
        public string Make(byte[] data)
        {
            var c = Init();
            Update(ref c, data, data.Length);
            var text = Final(ref c).ToHexString();
            return text;
        }
    }
}
