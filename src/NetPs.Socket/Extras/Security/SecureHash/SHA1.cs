namespace NetPs.Socket.Extras.Security.SecureHash
{
    using System;
    ///<remarks>
    /// https://mirrors.nju.edu.cn/rfc/beta/errata/rfc3174.html
    ///</remarks>
    public class SHA1
    {
        internal const uint HASH_ROUND_NUM = 80;
        internal const uint HASH_BLOCK_SIZE = 64;
        internal static uint ROTL(uint x, byte shift)
        {
            return (x << shift) | (x >> (32 - shift));
        }
        internal static uint F1(uint x, uint y, uint z)
        {
            return (x & y) ^ (~x & z);
        }
        internal static uint F2(uint x, uint y, uint z)
        {
            return x ^ y ^ z;
        }
        internal static uint F3(uint x, uint y, uint z)
        {
            return (x & y) ^ (x & z) ^ (y & z);
        }
        internal delegate uint Func(uint x, uint y, uint z);
        private static SHA1_CTX Init()
        {
            var ctx = new SHA1_CTX();
            ctx.a = 0x67452301;
            ctx.b = 0xEFCDAB89;
            ctx.c = 0x98BADCFE;
            ctx.d = 0x10325476;
            ctx.e = 0xC3D2E1F0;
            ctx.total = 0;
            ctx.used = 0;
            ctx.buf = new uint[16];
            return ctx;
        }
        internal static readonly uint[] K = { 0x5A827999, 0x6ED9EBA1, 0x8F1BBCDC, 0xCA62C1D6 };
        internal static readonly Func[] F = { F1, F2, F3, F2 };
        private static void ProcessBlock(ref SHA1_CTX ctx, byte[] block)
        {
            uint t,s, temp;
            uint a, b, c, d, e;
            a = ctx.a;
            b = ctx.b;
            c = ctx.c;
            d = ctx.d;
            e = ctx.e;
            for (t = 0; t<HASH_ROUND_NUM; t++)
            {
                s = t & 0xf;
                if (t > 0xf)
                {
                    ctx.buf[s] = ROTL(ctx.buf[(t - 3)&0xf] ^ ctx.buf[(t - 8)&0xf] ^ ctx.buf[(t - 14)&0xf] ^ ctx.buf[(t - 16) & 0xf], 1);
                }

                temp = ROTL(a, 5) + F[t/20](b, c ,d) + e + K[t / 20] + ctx.buf[s];
                e = d;
                d = c;
                c = ROTL(b, 30);
                b = a;
                a = temp;
            }
            ctx.a += a;
            ctx.b += b;
            ctx.c += c;
            ctx.d += d;
            ctx.e += e;
        }
        private static void Update(ref SHA1_CTX ctx, byte[] data, int length)
        {
            if (ctx.used != 0)
            {
                if (ctx.used + length < HASH_BLOCK_SIZE)
                {

                }
                else
                {

                }
            }
        }
        private static byte[] Final(ref SHA1_CTX ctx)
        {

            return null;
        }
        public string Make(byte[] data)
        {
            var ctx = Init();
            Update(ref ctx, data, data.Length);
            Final(ref ctx);
            var text = string.Empty;
            return text;
        }
    }
}
