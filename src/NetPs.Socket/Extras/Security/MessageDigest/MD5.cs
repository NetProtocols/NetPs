namespace NetPs.Socket.Extras.Security.MessageDigest
{
    using NetPs.Socket.Memory;
    using System;
    
    ///<remarks>
    ///http://mirrors.nju.edu.cn/rfc/beta/errata/rfc1321.html
    ///</remarks>
    public class MD5 : IHash
    {
        internal static readonly uint[] T =
        {
            0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee, 0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501,
            0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be, 0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821, 
            0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa, 0xd62f105d, 0x02441453, 0xd8a1e681, 0xe7d3fbc8,
            0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed, 0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a, 
            0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c, 0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70,
            0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x04881d05, 0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665, 
            0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039, 0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1,
            0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1, 0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391,
        };
        internal static uint G(uint x, uint y, uint z)
        {
            return (x & z) | (y & (~z));
        }
        internal static uint I(uint x, uint y, uint z)
        {
            return y ^ (x | (~z));
        }
        private static MD5_CTX Init()
        {
            var c = new MD5_CTX();
            c.a = 0x67452301;
            c.b = 0xEFCDAB89;
            c.c = 0x98BADCFE;
            c.d = 0x10325476;
            c.buffer = uint_buf_reverse.New(16);
            return c;
        }
        private static void ProcessBlock(ref MD5_CTX ctx)
        {
            uint a, b, c, d;
            MD4.RoundFunc Func;
            uint[] buf = ctx.buf;
            a = ctx.a;
            b = ctx.b;
            c = ctx.c;
            d = ctx.d;

            uint MD5_OP(uint a, uint b, uint c, uint d, byte k, byte s, byte i)
            {
                return b + MD4.ROTL(a + Func(b, c, d) + buf[k] + T[i - 1], s);
            }
            Func = MD4.F;
            a = MD5_OP(a, b, c, d, 0, 7, 1);   d = MD5_OP(d, a, b, c, 1, 12, 2);   c = MD5_OP(c, d, a, b, 2, 17, 3);   b = MD5_OP(b, c, d, a, 3, 22, 4);
            a = MD5_OP(a, b, c, d, 4, 7, 5);   d = MD5_OP(d, a, b, c, 5, 12, 6);   c = MD5_OP(c, d, a, b, 6, 17, 7);   b = MD5_OP(b, c, d, a, 7, 22, 8);
            a = MD5_OP(a, b, c, d, 8, 7, 9);   d = MD5_OP(d, a, b, c, 9, 12, 10);  c = MD5_OP(c, d, a, b, 10, 17, 11); b = MD5_OP(b, c, d, a, 11, 22, 12);
            a = MD5_OP(a, b, c, d, 12, 7, 13); d = MD5_OP(d, a, b, c, 13, 12, 14); c = MD5_OP(c, d, a, b, 14, 17, 15); b = MD5_OP(b, c, d, a, 15, 22, 16);
            Func = G;
            a = MD5_OP(a, b, c, d, 1, 5, 17);  d = MD5_OP(d, a, b, c, 6, 9, 18);  c = MD5_OP(c, d, a, b, 11, 14, 19); b = MD5_OP(b, c, d, a, 0, 20, 20);
            a = MD5_OP(a, b, c, d, 5, 5, 21);  d = MD5_OP(d, a, b, c, 10, 9, 22); c = MD5_OP(c, d, a, b, 15, 14, 23); b = MD5_OP(b, c, d, a, 4, 20, 24);
            a = MD5_OP(a, b, c, d, 9, 5, 25);  d = MD5_OP(d, a, b, c, 14, 9, 26); c = MD5_OP(c, d, a, b, 3, 14, 27);  b = MD5_OP(b, c, d, a, 8, 20, 28);
            a = MD5_OP(a, b, c, d, 13, 5, 29); d = MD5_OP(d, a, b, c, 2, 9, 30);  c = MD5_OP(c, d, a, b, 7, 14, 31);  b = MD5_OP(b, c, d, a, 12, 20, 32);
            Func = MD4.H;
            a = MD5_OP(a, b, c, d, 5, 4, 33);  d = MD5_OP(d, a, b, c, 8, 11, 34);  c = MD5_OP(c, d, a, b, 11, 16, 35); b = MD5_OP(b, c, d, a, 14, 23, 36);
            a = MD5_OP(a, b, c, d, 1, 4, 37);  d = MD5_OP(d, a, b, c, 4, 11, 38);  c = MD5_OP(c, d, a, b, 7, 16, 39);  b = MD5_OP(b, c, d, a, 10, 23, 40);
            a = MD5_OP(a, b, c, d, 13, 4, 41); d = MD5_OP(d, a, b, c, 0, 11, 42);  c = MD5_OP(c, d, a, b, 3, 16, 43);  b = MD5_OP(b, c, d, a, 6, 23, 44);
            a = MD5_OP(a, b, c, d, 9, 4, 45);  d = MD5_OP(d, a, b, c, 12, 11, 46); c = MD5_OP(c, d, a, b, 15, 16, 47); b = MD5_OP(b, c, d, a, 2, 23, 48);
            Func = I;
            a = MD5_OP(a, b, c, d, 0, 6, 49);  d = MD5_OP(d, a, b, c, 7, 10, 50);  c = MD5_OP(c, d, a, b, 14, 15, 51); b = MD5_OP(b, c, d, a, 5, 21, 52);
            a = MD5_OP(a, b, c, d, 12, 6, 53); d = MD5_OP(d, a, b, c, 3, 10, 54);  c = MD5_OP(c, d, a, b, 10, 15, 55); b = MD5_OP(b, c, d, a, 1, 21, 56);
            a = MD5_OP(a, b, c, d, 8, 6, 57);  d = MD5_OP(d, a, b, c, 15, 10, 58); c = MD5_OP(c, d, a, b, 6, 15, 59);  b = MD5_OP(b, c, d, a, 13, 21, 60);
            a = MD5_OP(a, b, c, d, 4, 6, 61);  d = MD5_OP(d, a, b, c, 11, 10, 62); c = MD5_OP(c, d, a, b, 2, 15, 63);  b = MD5_OP(b, c, d, a, 9, 21, 64);
            
            ctx.a += a;
            ctx.b += b;
            ctx.c += c;
            ctx.d += d;
        }
        private static void Update(ref MD5_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref ctx);
                }
            }
        }
        private static byte[] Final(ref MD5_CTX ctx)
        {
            if (ctx.buffer.IsFULL()) ProcessBlock(ref ctx);
            ctx.buffer.PushNext(0x80);
            if (ctx.buffer.IsFULL(2)) ProcessBlock(ref ctx);
            ctx.buffer.Fill(0, 2);
            ctx.buffer.PushTotal();
            ProcessBlock(ref ctx);

            var md = new byte[16];
            int i = 0;
            md.CopyFrom(ctx.a, i++ << 2);
            md.CopyFrom(ctx.b, i++ << 2);
            md.CopyFrom(ctx.c, i++ << 2);
            md.CopyFrom(ctx.d, i++ << 2);
            return md;
        }
        public string Make(byte[] data)
        {
            var c = Init();
            Update(ref c, data, data.Length);
            return Final(ref c).ToHexString();
        }
    }
}
