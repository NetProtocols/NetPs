namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;
    /// <remarks>
    /// https://www2.seas.gwu.edu/~poorvi/Classes/CS381_2007/Whirlpool.pdf
    /// </remarks>
    public class WHIRLPOOL : IHash
    {
        private const string t_SBox = "\u1823\uc6E8\u87B8\u014F\u36A6\ud2F5\u796F\u9152" + "\u60Bc\u9B8E\uA30c\u7B35\u1dE0\ud7c2\u2E4B\uFE57" + "\u1577\u37E5\u9FF0\u4AdA\u58c9\u290A\uB1A0\u6B85" + "\uBd5d\u10F4\ucB3E\u0567\uE427\u418B\uA77d\u95d8" + "\uFBEE\u7c66\udd17\u479E\ucA2d\uBF07\uAd5A\u8333" + "\u6302\uAA71\uc819\u49d9\uF2E3\u5B88\u9A26\u32B0" + "\uE90F\ud580\uBEcd\u3448\uFF7A\u905F\u2068\u1AAE" + "\uB454\u9322\u64F1\u7312\u4008\uc3Ec\udBA1\u8d3d" + "\u9700\ucF2B\u7682\ud61B\uB5AF\u6A50\u45F3\u30EF" + "\u3F55\uA2EA\u65BA\u2Fc0\udE1c\uFd4d\u9275\u068A" + "\uB2E6\u0E1F\u62d4\uA896\uF9c5\u2559\u8472\u394c" + "\u5E78\u388c\ud1A5\uE261\uB321\u9c1E\u43c7\uFc04" + "\u5199\u6d0d\uFAdF\u7E24\u3BAB\ucE11\u8F4E\uB7EB" + "\u3c81\u94F7\uB913\u2cd3\uE76E\uc403\u5644\u7FA9" + "\u2ABB\uc153\udc0B\u9d6c\u3174\uF646\uAc89\u14E1" + "\u163A\u6909\u70B6\ud0Ed\ucc42\u98A4\u285c\uF886";
        //ulong[8][256]
        internal static ulong[][] Init_SBox()
        {
            var c = new ulong[8][];
            int i, t;
            uint v1, v2, v4, v5, v8, v9;
            ushort chr;
            for (i = 8; i-- > 0;) c[i] = new ulong[256];
            for (i = 0; i < 256; i++)
            {
                chr = t_SBox[i / 2];
                if ((i & 1) == 0) v1 = (uint)(chr >>> 8);
                else v1 = (uint)(chr & 0xff);

                v2 = v1 << 1;
                if (v2 >= 0x100) v2 ^= 0x11d;

                v4 = v2 << 1;
                if (v4 >= 0x100) v4 ^= 0x11d;

                v5 = v4 ^ v1;
                v8 = v4 << 1;
                if (v8 >= 0x100) v8 ^= 0x11d;
                v9 = v8 ^ v1;

                c[0][i] = (ulong)(((ulong)v1 << 56) | ((ulong)v1 << 48) | ((ulong)v4 << 40) | ((ulong)v1 << 32) | (v8 << 24) | (v5 << 16) | (v2 << 8) | v9);

                for (t = 1; t < 8; t++) c[t][i] = (c[t-1][i] >> 8) | (c[t-1][i] << 56);
            }
            return c;
        }
        internal static readonly ulong[] RC = { 0x1823c6e887b8014f, 0x36a6d2f5796f9152, 0x60bc9b8ea30c7b35, 0x1de0d7c22e4bfe57, 0x157737e59ff04ada, 0x58c9290ab1a06b85, 0xbd5d10f4cb3e0567, 0xe427418ba77d95d8, 0xfbee7c66dd17479e, 0xca2dbf07ad5a8333, 0x6302aa71c81949d9 };
        internal static ulong[][] SBox = Init_SBox();
        internal static byte GB(ulong[] a, uint i, byte j) {
            return (byte)((a[(i) & 7] >> (8 * (j))) & 255);
        }
        internal static ulong theta_pi_gamma(ulong[] a, uint i)
        {
            return SBox[0][GB(a, i - 0, 7)] ^
            SBox[1][GB(a, i - 1, 6)] ^
            SBox[2][GB(a, i - 2, 5)] ^
            SBox[3][GB(a, i - 3, 4)] ^
            SBox[4][GB(a, i - 4, 3)] ^
            SBox[5][GB(a, i - 5, 2)] ^
            SBox[6][GB(a, i - 6, 1)] ^
            SBox[7][GB(a, i - 7, 0)];
        }
        internal static void ProcessBlock(ref WHIRLPOOL_CTX ctx)
        {
            ulong[][] K = new ulong[2][];
            ulong[][] T = new ulong[3][];
            K[0] = new ulong[8];
            K[1] = new ulong[8];
            T[0] = new ulong[8];
            T[1] = new ulong[8];
            T[2] = new ulong[8];

            uint x, y;
            for (x = 0; x < 8; x++)
            {
                K[0][x] = ctx.state[x];
                T[0][x] = ctx.buf[x];
                T[2][x] = T[0][x];
                T[0][x] ^= K[0][x];
            }

            for (x = 0; x < 10; x += 2)
            {
                for (y = 0; y < 8; y++)
                {
                    K[1][y] = theta_pi_gamma(K[0], y);
                }
                K[1][0] ^= RC[x];

                for (y = 0; y < 8; y++)
                {
                    T[1][y] = theta_pi_gamma(T[0], y) ^ K[1][y];
                }

                for (y = 0; y < 8; y++)
                {
                    K[0][y] = theta_pi_gamma(K[1], y);
                }
                K[0][0] ^= RC[x+1];

                for (y = 0; y < 8; y++)
                {
                    T[0][y] = theta_pi_gamma(T[1], y) ^ K[0][y];
                }
            }

            for (x = 0; x < 8; x++)
            {
                ctx.state[x] ^= T[0][x] ^ T[2][x];
            }
        }
        internal static WHIRLPOOL_CTX Init()
        {
            var ctx = new WHIRLPOOL_CTX();
            ctx.buffer = ulong_buf.New(8);
            ctx.state = new ulong[8];
            return ctx;
        }
        internal static void Update(ref WHIRLPOOL_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static byte[] Final(ref WHIRLPOOL_CTX ctx)
        {
            if (ctx.buffer.IsFULL()) ProcessBlock(ref ctx);
            ctx.buffer.PushNext(0x80);
            if (ctx.buffer.IsFULL(2)) ProcessBlock(ref ctx);
            ctx.buffer.Fill(0, 2);
            ctx.buffer.PushTotal();
            ProcessBlock(ref ctx);

            byte[] md = new byte[64];
            for (byte i = 0; i < 8; i++)
            md.CopyFrom_Reverse(ctx.state[i], i * 8);
            return md;
        }
        public string Make(byte[] data)
        {
            var ctx = Init();
            Update(ref ctx, data, data.Length);
            return Final(ref ctx).ToHexString();
        }
    }
}
