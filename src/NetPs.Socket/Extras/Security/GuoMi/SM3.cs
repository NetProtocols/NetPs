namespace NetPs.Socket.Extras.Security.GuoMi
{
    using NetPs.Socket.Memory;
    using System;
    /// <summary>
    /// http://www.gmbz.org.cn/main/bzlb.html
    /// </summary>
    public class SM3
    {
        internal const uint HASH_ROUND_NUM = 64;
        internal static uint[] T = { 0x79CC4519, 0x7A879D8A };
        internal static uint ROTL(uint x, byte shift)
        {
            shift %= 32;
            return (x << shift) | (x >> (32 - shift));
        }
        internal static uint FF(uint x, uint y, uint z, uint j)
        {
            if (j < 16)
            {
                return x ^ y ^ z;
            }
            else
            {
                return (x & y) | (x & z) | (y & z);
            }
        }
        internal static uint GG(uint x, uint y, uint z, uint j)
        {
            if (j < 16)
            {
                return x ^ y ^ z;
            }
            else
            {
                return (x & y) | (~x & z);
            }
        }
        internal static uint P0(uint x)
        {
            return x ^ ROTL(x, 9) ^ ROTL(x, 17);
        }
        internal static uint P1(uint x)
        {
            return x ^ ROTL(x, 15) ^ ROTL(x, 23);
        }

        internal static SM3_CTX Init()
        {
            var ctx = new SM3_CTX();
            ctx.buffer = uint_buf.New(16);
            ctx.a = 0x7380166f;
            ctx.b = 0x4914b2b9;
            ctx.c = 0x172442d7;
            ctx.d = 0xda8a0600;
            ctx.e = 0xa96f30bc;
            ctx.f = 0x163138aa;
            ctx.g = 0xe38dee4d;
            ctx.h = 0xb0fb0e4e;

            return ctx;
        }
        internal static void ProcessBlock(ref SM3_CTX ctx)
        {
            byte j;
            uint S1, S2, T1, T2;
            uint a, b, c, d, e, f, g, h;
            uint[] W = new uint[HASH_ROUND_NUM+4];
            a = ctx.a;
            b = ctx.b;
            c = ctx.c;
            d = ctx.d;
            e = ctx.e;
            f = ctx.f;
            g = ctx.g;
            h = ctx.h;
            for (j = 0; j < HASH_ROUND_NUM +4; j++)
            {
                if (j <= 0xf) W[j] = ctx.buf[j];
                else W[j] = P1(W[j - 16] ^ W[j - 9] ^ ROTL(W[j - 3], 15)) ^ ROTL(W[j - 13], 7) ^ W[j - 6];
            }

            for (j = 0; j < HASH_ROUND_NUM; j++)
            {
                S1 = ROTL(ROTL(a, 12) + e + ROTL(T[j < 16 ? 0 : 1], j), 7);
                S2 = S1 ^ ROTL(a, 12);
                T1 = FF(a, b, c, j) + d + S2 + (W[j] ^ W[j+4]);
                T2 = GG(e, f, g, j) + h + S1 + W[j];
                d = c;
                c = ROTL(b, 9);
                b = a;
                a = T1;
                h = g;
                g = ROTL(f, 19);
                f = e;
                e = P0(T2);
            }

            ctx.a ^= a;
            ctx.b ^= b;
            ctx.c ^= c;
            ctx.d ^= d;
            ctx.e ^= e;
            ctx.f ^= f;
            ctx.g ^= g;
            ctx.h ^= h;
        }

        internal static void Update(ref SM3_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static byte[] Final(ref SM3_CTX ctx)
        {
            if (ctx.buffer.NotFirstFull) ProcessBlock(ref ctx);
            ctx.buffer.PushNext(0x80);
            if (ctx.buffer.NotFirstFull) ProcessBlock(ref ctx);
            ctx.buffer.Fill(0, 2);
            ctx.buffer.PushTotal();
            ProcessBlock(ref ctx);

            byte[] sha = new byte[32];
            sha.CopyFrom_Reverse(ctx.a, 0);
            sha.CopyFrom_Reverse(ctx.b, 4);
            sha.CopyFrom_Reverse(ctx.c, 8);
            sha.CopyFrom_Reverse(ctx.d, 12);
            sha.CopyFrom_Reverse(ctx.e, 16);
            sha.CopyFrom_Reverse(ctx.f, 20);
            sha.CopyFrom_Reverse(ctx.g, 24);
            sha.CopyFrom_Reverse(ctx.h, 28);
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
