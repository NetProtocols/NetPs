namespace NetPs.Socket.Extras.Security.MessageDigest
{
    using NetPs.Socket.Memory;
    using System;

    ///<remarks>
    ///http://mirrors.nju.edu.cn/rfc/beta/errata/rfc1320.html
    ///</remarks>
    public class MD4 : IHash
    {
        internal delegate uint RoundFunc(uint x, uint y, uint z);
        internal static uint F(uint x, uint y, uint z)
        {
            return (x & y) | ((~x) & z);
        }
        internal static uint G(uint x, uint y, uint z)
        {
            return (x & y) | (x & z) | (y & z);
        }
        internal static uint H(uint x, uint y, uint z)
        {
            return x ^ y ^ z;
        }
        internal static uint ROTL(uint x, byte shift)
        {
            return (x << shift) | (x >> (32 - shift));
        }
        
        private void ProcessBlock(ref MD4_CTX ctx)
        {
            uint[] buf = ctx.buf;
            var A = ctx.a;
            var B = ctx.b;
            var C = ctx.c;
            var D = ctx.d;
            uint T;
            RoundFunc Func;
            uint MD4_OP(uint a, uint b, uint c, uint d, int k, byte s)
            {
                return ROTL(a + Func(b, c, d)+ buf[k] + T, s);
            }
            T = 0;
            Func = F;
            A = MD4_OP(A, B, C, D, 0, 3);  D=MD4_OP(D, A, B, C, 1, 7);  C=MD4_OP(C, D, A, B, 2, 11);  B=MD4_OP(B, C, D, A, 3, 19);
            A = MD4_OP(A, B, C, D, 4, 3);  D=MD4_OP(D, A, B, C, 5, 7);  C=MD4_OP(C, D, A, B, 6, 11);  B=MD4_OP(B, C, D, A, 7, 19);
            A = MD4_OP(A, B, C, D, 8, 3);  D=MD4_OP(D, A, B, C, 9, 7);  C=MD4_OP(C, D, A, B, 10, 11); B=MD4_OP(B, C, D, A, 11, 19);
            A = MD4_OP(A, B, C, D, 12, 3); D=MD4_OP(D, A, B, C, 13, 7); C=MD4_OP(C, D, A, B, 14, 11); B=MD4_OP(B, C, D, A, 15, 19);
            T = 0x5A827999;
            Func = G;
            A = MD4_OP(A, B, C, D, 0, 3); D=MD4_OP(D, A, B, C, 4, 5); C=MD4_OP(C, D, A, B, 8, 9);  B=MD4_OP(B, C, D, A, 12, 13);
            A = MD4_OP(A, B, C, D, 1, 3); D=MD4_OP(D, A, B, C, 5, 5); C=MD4_OP(C, D, A, B, 9, 9);  B=MD4_OP(B, C, D, A, 13, 13);
            A = MD4_OP(A, B, C, D, 2, 3); D=MD4_OP(D, A, B, C, 6, 5); C=MD4_OP(C, D, A, B, 10, 9); B=MD4_OP(B, C, D, A, 14, 13);
            A = MD4_OP(A, B, C, D, 3, 3); D=MD4_OP(D, A, B, C, 7, 5); C=MD4_OP(C, D, A, B, 11, 9); B=MD4_OP(B, C, D, A, 15, 13);
            T = 0x6ED9EBA1;
            Func = H;
            A = MD4_OP(A, B, C, D, 0, 3); D=MD4_OP(D, A, B, C, 8, 9);  C=MD4_OP(C, D, A, B, 4, 11); B=MD4_OP(B, C, D, A, 12, 15);
            A = MD4_OP(A, B, C, D, 2, 3); D=MD4_OP(D, A, B, C, 10, 9); C=MD4_OP(C, D, A, B, 6, 11); B=MD4_OP(B, C, D, A, 14, 15);
            A = MD4_OP(A, B, C, D, 1, 3); D=MD4_OP(D, A, B, C, 9, 9);  C=MD4_OP(C, D, A, B, 5, 11); B=MD4_OP(B, C, D, A, 13, 15);
            A = MD4_OP(A, B, C, D, 3, 3); D=MD4_OP(D, A, B, C, 11, 9); C=MD4_OP(C, D, A, B, 7, 11); B=MD4_OP(B, C, D, A, 15, 15);
            ctx.a += A;
            ctx.b += B;
            ctx.c += C;
            ctx.d += D;
        }
        private void Update(ref MD4_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref ctx);
                }
            }
        }
        private byte[] Final(ref MD4_CTX ctx)
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
        private MD4_CTX Init()
        {
            var ctx = new MD4_CTX();
            ctx.a = 0x67452301;
            ctx.b = 0xEFCDAB89;
            ctx.c = 0x98BADCFE;
            ctx.d = 0x10325476;
            ctx.buffer = uint_buf_reverse.New(16);
            return ctx;
        }
        public string Make(byte[] data)
        {
            var ctx = Init();
            Update(ref ctx, data, data.Length);
            return Final(ref ctx).ToHexString();
        }
    }
}
