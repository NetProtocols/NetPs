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
        internal const byte PADDING = 0b10000000;
        internal static uint F1(uint b, uint c, uint d)
        {
            return (b & c) | (~b & c);
        }
        internal static uint F2(uint b, uint c, uint d)
        {
            return b ^ c ^ d;
        }
        internal static uint F3(uint b, uint c, uint d)
        {
            return (b & c) | (b & d) | (c & d);
        }
        internal static uint S(uint x, byte shift)
        {
            return (x << shift) | (x >> (32 - shift));
        }
        internal static readonly uint[] H = { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476, 0xC3D2E1F0 };
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
        internal static void ProcessBlock(ref SHA0_CTX ctx)
        {
            uint i, temp;
            for (i = 16; i < 80; i++)
            {
                ctx.buf[i] = ctx.buf[i - 3]^ ctx.buf[i - 8] ^ ctx.buf[i - 14] ^ ctx.buf[i -16];
            }
            uint a, b, c, d, e;
            a = ctx.a;
            b = ctx.b;
            c = ctx.c;
            d = ctx.d;
            e = ctx.e;
            for (i = 0; i < 80; i++)
            {
                temp = S(a, 5) + e + ctx.buf[i];
                if (i < 20)
                {
                    temp += 0x5A827999 + F1(b, c, d);
                }
                else if (i< 40)
                {
                    temp += 0x6ED9EBA1 + F2(b, c, d);
                }
                else if (i < 60)
                {
                    temp += 0x8F1BBCDC + F3(b, c, d);
                }
                else
                {
                    temp += 0xCA62C1D6 + F2(b, c, d);
                }
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
            uint i= 0, j = c.used;
            do
            {
                if (j > 0)
                {
                    if ((c.total & 0b11) != 0)
                    {
                        if ((c.total & 0b10) != 0)
                        {
                            if ((c.total & 0b1) == 0)
                            {
                                c.buf[j] |= (byte)((data[i++] >> 16) & 0xff);
                                if (length == i) break;
                            }
                            c.buf[j] |= (byte)((data[i++] >> 8) & 0xff);
                            if (length == i) break;
                        }
                        c.buf[j] |= (byte)((data[i++]) & 0xff);
                        c.used++;
                        if (length == i) break;
                    }
                }
            } while (false);
            c.total += length;

            for (; i < length; i+= 4, j++)
            {
                if (j == WORD_SIZE >> 5)
                {
                    ProcessBlock(ref c);
                }
            }
        }
        internal static void Final(ref SHA0_CTX c)
        {

        }
        public string Make(byte[] data)
        {
            var c = Init();
            Update(ref c, data, data.Length);
            var text = string.Empty;
            return text;
        }
    }
}
