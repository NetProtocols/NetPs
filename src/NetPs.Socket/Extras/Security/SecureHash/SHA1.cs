namespace NetPs.Socket.Extras.Security.SecureHash
{
    using NetPs.Socket.Memory;
    using System;
    ///<remarks>
    /// https://mirrors.nju.edu.cn/rfc/beta/errata/rfc3174.html
    ///</remarks>
    public class SHA1
    {
        internal const uint HASH_ROUND_NUM = 80;
        internal const uint HASH_BLOCK_SIZE = 64;
        internal const uint HASH_LEN_SIZE = 8;
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
            ctx.buffer = uint_buf.New(16);
            ctx.a = 0x67452301;
            ctx.b = 0xEFCDAB89;
            ctx.c = 0x98BADCFE;
            ctx.d = 0x10325476;
            ctx.e = 0xC3D2E1F0;
            return ctx;
        }
        internal static readonly uint[] K = { 0x5A827999, 0x6ED9EBA1, 0x8F1BBCDC, 0xCA62C1D6 };
        internal static readonly Func[] F = { F1, F2, F3, F2 };
        private static void ProcessBlock(ref SHA1_CTX ctx)
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
                    ctx.buf[s] = ROTL(ctx.buf[(s + 13)&0xf] ^ ctx.buf[(s +8)&0xf] ^ ctx.buf[(s + 2)&0xf] ^ ctx.buf[(s)], 1);
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
        private static void Update(ref SHA1_CTX c, byte[] data, int length)
        {
            foreach(uint i in c.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref c);
                }
            }
        }
        private static byte[] Final(ref SHA1_CTX c)
        {
            if (c.buffer.IsFULL()) ProcessBlock(ref c);
            c.buffer.PushNext(0x80);
            if (c.buffer.IsFULL(2)) ProcessBlock(ref c);
            c.buffer.Fill(0, 2);
            c.buffer.PushTotal();
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
            var ctx = Init();
            Update(ref ctx, data, data.Length);
            return Final(ref ctx).ToHexString();
        }
    }
}
