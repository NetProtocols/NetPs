namespace NetPs.Socket.Extras.Security.SecureHash
{
    using NetPs.Socket.Memory;
    using System;
    ///<remarks>
    /// https://csrc.nist.gov/files/pubs/fips/180-4/final/docs/fips180-4.pdf
    ///</remarks>
    internal readonly struct SHA2
    {
        internal const byte SHA256_ROUND_NUM = 64;
        internal const byte SHA512_ROUND_NUM = 80;
        internal static readonly uint[] K256 = {
            0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
            0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
            0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
            0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
            0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
            0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
            0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
            0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
            0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
            0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
            0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
            0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
            0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
            0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
            0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
            0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
        };
        internal static ulong[] K512 =
        {
            0x428a2f98d728ae22, 0x7137449123ef65cd, 0xb5c0fbcfec4d3b2f, 0xe9b5dba58189dbbc,
            0x3956c25bf348b538, 0x59f111f1b605d019, 0x923f82a4af194f9b, 0xab1c5ed5da6d8118,
            0xd807aa98a3030242, 0x12835b0145706fbe, 0x243185be4ee4b28c, 0x550c7dc3d5ffb4e2,
            0x72be5d74f27b896f, 0x80deb1fe3b1696b1, 0x9bdc06a725c71235, 0xc19bf174cf692694,
            0xe49b69c19ef14ad2, 0xefbe4786384f25e3, 0x0fc19dc68b8cd5b5, 0x240ca1cc77ac9c65,
            0x2de92c6f592b0275, 0x4a7484aa6ea6e483, 0x5cb0a9dcbd41fbd4, 0x76f988da831153b5,
            0x983e5152ee66dfab, 0xa831c66d2db43210, 0xb00327c898fb213f, 0xbf597fc7beef0ee4,
            0xc6e00bf33da88fc2, 0xd5a79147930aa725, 0x06ca6351e003826f, 0x142929670a0e6e70,
            0x27b70a8546d22ffc, 0x2e1b21385c26c926, 0x4d2c6dfc5ac42aed, 0x53380d139d95b3df,
            0x650a73548baf63de, 0x766a0abb3c77b2a8, 0x81c2c92e47edaee6, 0x92722c851482353b,
            0xa2bfe8a14cf10364, 0xa81a664bbc423001, 0xc24b8b70d0f89791, 0xc76c51a30654be30,
            0xd192e819d6ef5218, 0xd69906245565a910, 0xf40e35855771202a, 0x106aa07032bbd1b8,
            0x19a4c116b8d2d0c8, 0x1e376c085141ab53, 0x2748774cdf8eeb99, 0x34b0bcb5e19b48a8,
            0x391c0cb3c5c95a63, 0x4ed8aa4ae3418acb, 0x5b9cca4f7763e373, 0x682e6ff3d6b2b8a3,
            0x748f82ee5defb2fc, 0x78a5636f43172f60, 0x84c87814a1f0ab72, 0x8cc702081a6439ec,
            0x90befffa23631e28, 0xa4506cebde82bde9, 0xbef9a3f7b2c67915, 0xc67178f2e372532b,
            0xca273eceea26619c, 0xd186b8c721c0c207, 0xeada7dd6cde0eb1e, 0xf57d4f7fee6ed178,
            0x06f067aa72176fba, 0x0a637dc5a2c898a6, 0x113f9804bef90dae, 0x1b710b35131c471b,
            0x28db77f523047d84, 0x32caab7b40c72493, 0x3c9ebe0a15c9bebc, 0x431d67c49c100d4c,
            0x4cc5d4becb3e42b6, 0x597f299cfc657e2a, 0x5fcb6fab3ad6faec, 0x6c44198c4a475817
        };
        internal static uint ROTR(uint x, byte shift)
        {
            return (x >> shift) | (x << (32 - shift));
        }
        internal static uint SHR(uint x, byte shift)
        {
            return (x >> shift);
        }
        internal static uint Ch(uint x, uint y, uint z)
        {
            return (x & y) ^ (~x & z);
        }
        internal static uint Maj(uint x, uint y, uint z)
        {
            return (x & y) ^ (x & z) ^ (y & z);
        }
        internal static uint SIGMA0(uint x)
        {
            return ROTR(x, 2) ^ ROTR(x, 13) ^ ROTR(x, 22);
        }
        internal static uint SIGMA1(uint x)
        {
            return ROTR(x, 6) ^ ROTR(x, 11) ^ ROTR(x, 25);
        }
        internal static uint sigma0(uint x)
        {
            return ROTR(x, 7) ^ ROTR(x, 18) ^ SHR(x, 3);
        }
        internal static uint sigma1(uint x)
        {
            return ROTR(x, 17) ^ ROTR(x, 19) ^ SHR(x, 10);
        }
        internal static ulong ROTR(ulong x, byte shift)
        {
            return (x >> shift) | (x << (64 - shift));
        }
        internal static ulong SHR(ulong x, byte shift)
        {
            return (x >> shift);
        }
        internal static ulong Ch(ulong x, ulong y, ulong z)
        {
            return (x & y) ^ (~x & z);
        }
        internal static ulong Maj(ulong x, ulong y, ulong z)
        {
            return (x & y) ^ (x & z) ^ (y & z);
        }
        internal static ulong SIGMA0(ulong x)
        {
            return ROTR(x, 28) ^ ROTR(x, 34) ^ ROTR(x, 39);
        }
        internal static ulong SIGMA1(ulong x)
        {
            return ROTR(x, 14) ^ ROTR(x, 18) ^ ROTR(x, 41);
        }
        internal static ulong sigma0(ulong x)
        {
            return ROTR(x, 1) ^ ROTR(x, 8) ^ SHR(x, 7);
        }
        internal static ulong sigma1(ulong x)
        {
            return ROTR(x, 19) ^ ROTR(x, 61) ^ SHR(x, 6);
        }
        internal static void ProcessBlock(ref SHA256_CTX ctx)
        {
            uint t, s;
            uint temp1, temp2;
            uint a, b, c, d, e, f, g, h;
            a = ctx.a;
            b = ctx.b;
            c = ctx.c;
            d = ctx.d;
            e = ctx.e;
            f = ctx.f;
            g = ctx.g;
            h = ctx.h;

            for (t = 0; t < SHA256_ROUND_NUM; t++)
            {
                s = t & 0xf;
                if (t > 0xf)
                {
                    ctx.buf[s] = sigma1(ctx.buf[(s + 14) & 0xf]) + ctx.buf[(s + 9) & 0xf] + sigma0(ctx.buf[(s + 1) & 0xf]) + ctx.buf[(s)];
                }
                temp1 = h + SIGMA1(e) + Ch(e, f, g) + K256[t] + ctx.buf[s];
                temp2 = SIGMA0(a) + Maj(a, b, c);
                h = g;
                g = f;
                f = e;
                e = d + temp1;
                d = c;
                c = b;
                b = a;
                a = temp1 + temp2;
            }
            ctx.a += a;
            ctx.b += b;
            ctx.c += c;
            ctx.d += d;
            ctx.e += e;
            ctx.f += f;
            ctx.g += g;
            ctx.h += h;
        }
        internal static void ProcessBlock(ref SHA512_CTX ctx)
        {
            uint t, s;
            ulong temp1, temp2;
            ulong a, b, c, d, e, f, g, h;
            a = ctx.a;
            b = ctx.b;
            c = ctx.c;
            d = ctx.d;
            e = ctx.e;
            f = ctx.f;
            g = ctx.g;
            h = ctx.h;

            for (t = 0; t < SHA512_ROUND_NUM; t++)
            {
                s = t & 0xf;
                if (t > 0xf)
                {
                    ctx.buf[s] = sigma1(ctx.buf[(s + 14) & 0xf]) + ctx.buf[(s + 9) & 0xf] + sigma0(ctx.buf[(s + 1) & 0xf]) + ctx.buf[(s)];
                }
                temp1 = h + SIGMA1(e) + Ch(e, f, g) + K512[t] + ctx.buf[s];
                temp2 = SIGMA0(a) + Maj(a, b, c);
                h = g;
                g = f;
                f = e;
                e = d + temp1;
                d = c;
                c = b;
                b = a;
                a = temp1 + temp2;
            }
            ctx.a += a;
            ctx.b += b;
            ctx.c += c;
            ctx.d += d;
            ctx.e += e;
            ctx.f += f;
            ctx.g += g;
            ctx.h += h;
        }
        internal static SHA256_CTX SHA256_Init()
        {
            var ctx = new SHA256_CTX();
            ctx.buffer = uint_buf.New(16);
            ctx.a = 0x6a09e667;
            ctx.b = 0xbb67ae85;
            ctx.c = 0x3c6ef372;
            ctx.d = 0xa54ff53a;
            ctx.e = 0x510e527f;
            ctx.f = 0x9b05688c;
            ctx.g = 0x1f83d9ab;
            ctx.h = 0x5be0cd19;
            ctx.size = 256;
            return ctx;
        }
        internal static SHA256_CTX SHA224_Init()
        {
            var ctx = new SHA256_CTX();
            ctx.buffer = uint_buf.New(16);
            ctx.a = 0xc1059ed8;
            ctx.b = 0x367cd507;
            ctx.c = 0x3070dd17;
            ctx.d = 0xf70e5939;
            ctx.e = 0xffc00b31;
            ctx.f = 0x68581511;
            ctx.g = 0x64f98fa7;
            ctx.h = 0xbefa4fa4;
            ctx.size = 224;
            return ctx;
        }
        internal static SHA512_CTX SHA384_Init()
        {
            var ctx = new SHA512_CTX();
            ctx.buffer = ulong_buf.New(16);
            ctx.a = 0xcbbb9d5dc1059ed8;
            ctx.b = 0x629a292a367cd507;
            ctx.c = 0x9159015a3070dd17;
            ctx.d = 0x152fecd8f70e5939;
            ctx.e = 0x67332667ffc00b31;
            ctx.f = 0x8eb44a8768581511;
            ctx.g = 0xdb0c2e0d64f98fa7;
            ctx.h = 0x47b5481dbefa4fa4;
            ctx.size = 384;
            return ctx;
        }
        internal static SHA512_CTX SHA512_224_Init()
        {
            var ctx = new SHA512_CTX();
            ctx.buffer = ulong_buf.New(16);
            ctx.a = 0x8c3d37c819544da2;
            ctx.b = 0x73e1996689dcd4d6;
            ctx.c = 0x1dfab7ae32ff9c82;
            ctx.d = 0x679dd514582f9fcf;
            ctx.e = 0x0f6d2b697bd44da8;
            ctx.f = 0x77e36f7304c48942;
            ctx.g = 0x3f9d85a86a1d36c8;
            ctx.h = 0x1112e6ad91d692a1;
            ctx.size = 224;
            return ctx;
        }
        internal static SHA512_CTX SHA512_256_Init()
        {
            var ctx = new SHA512_CTX();
            ctx.buffer = ulong_buf.New(16);
            ctx.a = 0x22312194fc2bf72c;
            ctx.b = 0x9f555fa3c84c64c2;
            ctx.c = 0x2393b86b6f53b151;
            ctx.d = 0x963877195940eabd;
            ctx.e = 0x96283ee2a88effe3;
            ctx.f = 0xbe5e1e2553863992;
            ctx.g = 0x2b0199fc2c85b8aa;
            ctx.h = 0x0eb72ddc81c52ca2;
            ctx.size = 256;
            return ctx;
        }
        internal static SHA512_CTX SHA512_Init()
        {
            var ctx = new SHA512_CTX();
            ctx.buffer = ulong_buf.New(16);
            ctx.a = 0x6a09e667f3bcc908;
            ctx.b = 0xbb67ae8584caa73b;
            ctx.c = 0x3c6ef372fe94f82b;
            ctx.d = 0xa54ff53a5f1d36f1;
            ctx.e = 0x510e527fade682d1;
            ctx.f = 0x9b05688c2b3e6c1f;
            ctx.g = 0x1f83d9abfb41bd6b;
            ctx.h = 0x5be0cd19137e2179;
            ctx.size = 512;
            return ctx;
        }
        internal static SHA512_CTX SHA512_384_Init()
        {
            var ctx = new SHA512_CTX();
            ctx.buffer = ulong_buf.New(16);
            ctx.a = 0xcbbb9d5dc1059ed8;
            ctx.b = 0x629a292a367cd507;
            ctx.c = 0x9159015a3070dd17;
            ctx.d = 0x152fecd8f70e5939;
            ctx.e = 0x67332667ffc00b31;
            ctx.f = 0x8eb44a8768581511;
            ctx.g = 0xdb0c2e0d64f98fa7;
            ctx.h = 0x47b5481dbefa4fa4;
            ctx.size = 384;
            return ctx;
        }
        internal static void Update(ref SHA256_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static void Update(ref SHA512_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static byte[] Final(ref SHA256_CTX ctx)
        {
            if (ctx.buffer.IsFULL()) ProcessBlock(ref ctx);
            ctx.buffer.PushNext(0x80);
            if (ctx.buffer.IsFULL(2)) ProcessBlock(ref ctx);
            ctx.buffer.Fill(0, 2);
            ctx.buffer.PushTotal();
            ProcessBlock(ref ctx);

            byte[] sha = new byte[ctx.size >> 3];
            sha.CopyFrom_Reverse(ctx.a, 0);
            sha.CopyFrom_Reverse(ctx.b, 4);
            sha.CopyFrom_Reverse(ctx.c, 8);
            sha.CopyFrom_Reverse(ctx.d, 12);
            sha.CopyFrom_Reverse(ctx.e, 16);
            sha.CopyFrom_Reverse(ctx.f, 20);
            sha.CopyFrom_Reverse(ctx.g, 24);
            if (ctx.size == 256) sha.CopyFrom_Reverse(ctx.h, 28);
            return sha;
        }
        internal static byte[] Final(ref SHA512_CTX ctx)
        {
            if (ctx.buffer.IsFULL()) ProcessBlock(ref ctx);
            ctx.buffer.PushNext(0x80);
            if (ctx.buffer.IsFULL(2)) ProcessBlock(ref ctx);
            ctx.buffer.Fill(0, 2);
            ctx.buffer.PushTotal();
            ProcessBlock(ref ctx);

            byte[] sha = new byte[ctx.size>>3];
            sha.CopyFrom_Reverse(ctx.a, 0);
            sha.CopyFrom_Reverse(ctx.b, 8);
            sha.CopyFrom_Reverse(ctx.c, 16);
            sha.CopyFrom_Reverse(ctx.d, 24);
            sha.CopyFrom_Reverse(ctx.e, 32);
            sha.CopyFrom_Reverse(ctx.f, 40);
            if (ctx.size == 512)
            {
                sha.CopyFrom_Reverse(ctx.g, 48);
                sha.CopyFrom_Reverse(ctx.h, 56);
            }
            return sha;
        }
    }

    public class SHA224
    {
        public string Make(byte[] data)
        {
            var ctx = SHA2.SHA224_Init();
            SHA2.Update(ref ctx, data, data.Length);
            return SHA2.Final(ref ctx).ToHexString();
        }
    }
    public class SHA256
    {
        public string Make(byte[] data)
        {
            var ctx = SHA2.SHA256_Init();
            SHA2.Update(ref ctx, data, data.Length);
            return SHA2.Final(ref ctx).ToHexString();
        }
    }
    public class SHA384
    {
        public string Make(byte[] data)
        {
            var ctx = SHA2.SHA384_Init();
            SHA2.Update(ref ctx, data, data.Length);
            return SHA2.Final(ref ctx).ToHexString();
        }
    }
    public class SHA512
    {
        public string Make(byte[] data)
        {
            var ctx = SHA2.SHA512_Init();
            SHA2.Update(ref ctx, data, data.Length);
            return SHA2.Final(ref ctx).ToHexString();
        }
    }
}
