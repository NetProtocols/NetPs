namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;
    using System.Reactive.Linq.ObservableImpl;

    /// <remarks>
    /// https://datatracker.ietf.org/doc/html/rfc7693
    /// </remarks>
    internal struct BLAKE
    {
        internal static readonly uint[] iv256 = { 0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A, 0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19 };
        internal static readonly uint[] iv224 = { 0xC1059ED8, 0x367CD507, 0x3070DD17, 0xF70E5939, 0xFFC00B31, 0x68581511, 0x64F98FA7, 0xBEFA4FA4 };
        internal static readonly ulong[] iv512 = { 0x6A09E667F3BCC908, 0xBB67AE8584CAA73B, 0x3C6EF372FE94F82B, 0xA54FF53A5F1D36F1, 0x510E527FADE682D1, 0x9B05688C2B3E6C1F, 0x1F83D9ABFB41BD6B, 0x5BE0CD19137E2179 };
        internal static readonly ulong[] iv384 = { 0xCBBB9D5DC1059ED8, 0x629A292A367CD507, 0x9159015A3070DD17, 0x152FECD8F70E5939, 0x67332667FFC00B31, 0x8EB44A8768581511, 0xDB0C2E0D64F98FA7, 0x47B5481DBEFA4FA4 };
        internal static readonly uint[] C32 = { 0x243F6A88, 0x85A308D3, 0x13198A2E, 0x03707344, 0xA4093822, 0x299F31D0, 0x082EFA98, 0xEC4E6C89, 0x452821E6, 0x38D01377, 0xBE5466CF, 0x34E90C6C, 0xC0AC29B7, 0xC97C50DD, 0x3F84D5B5, 0xB5470917, };
        internal static readonly ulong[] C64 = { 0x243F6A8885A308D3, 0x13198A2E03707344, 0xA4093822299F31D0, 0x082EFA98EC4E6C89, 0x452821E638D01377, 0xBE5466CF34E90C6C, 0xC0AC29B7C97C50DD, 0x3F84D5B5B5470917, 0x9216D5D98979FB1B, 0xD1310BA698DFB5AC, 0x2FFD72DBD01ADFB7, 0xB8E1AFED6A267E96, 0xBA7C9045F12C7F99, 0x24A19947B3916CF7, 0x0801F2E2858EFC16, 0x636920D871574E69, };
        internal static readonly byte[] Z = { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF, 0xE, 0xA, 0x4, 0x8, 0x9, 0xF, 0xD, 0x6, 0x1, 0xC, 0x0, 0x2, 0xB, 0x7, 0x5, 0x3, 0xB, 0x8, 0xC, 0x0, 0x5, 0x2, 0xF, 0xD, 0xA, 0xE, 0x3, 0x6, 0x7, 0x1, 0x9, 0x4, 0x7, 0x9, 0x3, 0x1, 0xD, 0xC, 0xB, 0xE, 0x2, 0x6, 0x5, 0xA, 0x4, 0x0, 0xF, 0x8, 0x9, 0x0, 0x5, 0x7, 0x2, 0x4, 0xA, 0xF, 0xE, 0x1, 0xB, 0xC, 0x6, 0x8, 0x3, 0xD, 0x2, 0xC, 0x6, 0xA, 0x0, 0xB, 0x8, 0x3, 0x4, 0xD, 0x7, 0x5, 0xF, 0xE, 0x1, 0x9, 0xC, 0x5, 0x1, 0xF, 0xE, 0xD, 0x4, 0xA, 0x0, 0x7, 0x6, 0x3, 0x9, 0x2, 0x8, 0xB, 0xD, 0xB, 0x7, 0xE, 0xC, 0x1, 0x3, 0x9, 0x5, 0x0, 0xF, 0x4, 0x8, 0x6, 0x2, 0xA, 0x6, 0xF, 0xE, 0x9, 0xB, 0x3, 0x0, 0x8, 0xC, 0x2, 0xD, 0x7, 0x1, 0x4, 0xA, 0x5, 0xA, 0x2, 0x8, 0x4, 0x7, 0x6, 0x1, 0x5, 0xF, 0xB, 0x9, 0xE, 0x3, 0xC, 0xD, 0x0, };
        internal static uint ROTR32(uint x, byte shift)
        {
            return (x >> shift) ^ (x << (32 - shift));
        }
        internal static ulong ROTR64(ulong x, byte shift)
        {
            return (x >> shift) ^ (x << (64 - shift));
        }
        internal static void GS(uint m0, uint m1, uint c0, uint c1, ref uint a, ref uint b, ref uint c, ref uint d)
        {
            a += b + (m0 ^ c1);
            c += d = ROTR32(d ^ a, 16);
            b = ROTR32(b ^ c, 12);
            a += b + (m1 ^ c0);
            c += d = ROTR32(d ^ a, 8);
            b = ROTR32(b ^ c, 7);
        }
        internal static void GS(ulong m0, ulong m1, ulong c0, ulong c1, ref ulong a, ref ulong b, ref ulong c, ref ulong d)
        {
            a += b + (m0 ^ c1);
            c += d = ROTR64(d ^ a, 32);
            b = ROTR64(b ^ c, 25);
            a += b + (m1 ^ c0);
            c += d = ROTR64(d ^ a, 16);
            b = ROTR64(b ^ c, 11);
        }
        internal static void GS_2b(ulong m0, ulong m1, ref ulong a, ref ulong b, ref ulong c, ref ulong d)
        {
            a += b + m0;
            c += d = ROTR64(d ^ a, 32);
            b = ROTR64(b ^ c, 24);
            a += b + m1;
            c += d = ROTR64(d ^ a, 16);
            b = ROTR64(b ^ c, 63);
        }
        internal static void GS_2s(uint m0, uint m1, ref uint a, ref uint b, ref uint c, ref uint d)
        {
            a += b + m0;
            c += d = ROTR32(d ^ a, 16);
            b = ROTR32(b ^ c, 12);
            a += b + m1;
            c += d = ROTR32(d ^ a, 8);
            b = ROTR32(b ^ c, 7);
        }
        internal static void ROUND(uint r, ref uint[] v, uint[] M)
        {
            GS(M[Z[r     ]], M[Z[r+ 0x1]], C32[Z[r     ]], C32[Z[r+ 0x1]], ref v[0], ref v[4], ref v[0x8], ref v[0xC]);
		    GS(M[Z[r+ 0x2]], M[Z[r+ 0x3]], C32[Z[r+ 0x2]], C32[Z[r+ 0x3]], ref v[1], ref v[5], ref v[0x9], ref v[0xD]);
		    GS(M[Z[r+ 0x4]], M[Z[r+ 0x5]], C32[Z[r+ 0x4]], C32[Z[r+ 0x5]], ref v[2], ref v[6], ref v[0xA], ref v[0xE]);
		    GS(M[Z[r+ 0x6]], M[Z[r+ 0x7]], C32[Z[r+ 0x6]], C32[Z[r+ 0x7]], ref v[3], ref v[7], ref v[0xB], ref v[0xF]);
		    GS(M[Z[r+ 0x8]], M[Z[r+ 0x9]], C32[Z[r+ 0x8]], C32[Z[r+ 0x9]], ref v[0], ref v[5], ref v[0xA], ref v[0xF]);
		    GS(M[Z[r+ 0xA]], M[Z[r+ 0xB]], C32[Z[r+ 0xA]], C32[Z[r+ 0xB]], ref v[1], ref v[6], ref v[0xB], ref v[0xC]);
		    GS(M[Z[r+ 0xC]], M[Z[r+ 0xD]], C32[Z[r+ 0xC]], C32[Z[r+ 0xD]], ref v[2], ref v[7], ref v[0x8], ref v[0xD]);
		    GS(M[Z[r+ 0xE]], M[Z[r+ 0xF]], C32[Z[r+ 0xE]], C32[Z[r+ 0xF]], ref v[3], ref v[4], ref v[0x9], ref v[0xE]);
        }
        internal static void ROUND(uint r, ref ulong[] v, ulong[] M)
        {
            GS(M[Z[r      ]], M[Z[r + 0x1]], C64[Z[r      ]], C64[Z[r + 0x1]], ref v[0], ref v[4], ref v[0x8], ref v[0xC]);
            GS(M[Z[r + 0x2]], M[Z[r + 0x3]], C64[Z[r + 0x2]], C64[Z[r + 0x3]], ref v[1], ref v[5], ref v[0x9], ref v[0xD]);
            GS(M[Z[r + 0x4]], M[Z[r + 0x5]], C64[Z[r + 0x4]], C64[Z[r + 0x5]], ref v[2], ref v[6], ref v[0xA], ref v[0xE]);
            GS(M[Z[r + 0x6]], M[Z[r + 0x7]], C64[Z[r + 0x6]], C64[Z[r + 0x7]], ref v[3], ref v[7], ref v[0xB], ref v[0xF]);
            GS(M[Z[r + 0x8]], M[Z[r + 0x9]], C64[Z[r + 0x8]], C64[Z[r + 0x9]], ref v[0], ref v[5], ref v[0xA], ref v[0xF]);
            GS(M[Z[r + 0xA]], M[Z[r + 0xB]], C64[Z[r + 0xA]], C64[Z[r + 0xB]], ref v[1], ref v[6], ref v[0xB], ref v[0xC]);
            GS(M[Z[r + 0xC]], M[Z[r + 0xD]], C64[Z[r + 0xC]], C64[Z[r + 0xD]], ref v[2], ref v[7], ref v[0x8], ref v[0xD]);
            GS(M[Z[r + 0xE]], M[Z[r + 0xF]], C64[Z[r + 0xE]], C64[Z[r + 0xF]], ref v[3], ref v[4], ref v[0x9], ref v[0xE]);
        }
        internal static void ROUND_2b(uint r, ref ulong[] v, ulong[] M)
        {
            GS_2b(M[Z[r      ]], M[Z[r + 0x1]], ref v[0], ref v[4], ref v[0x8], ref v[0xC]);
            GS_2b(M[Z[r + 0x2]], M[Z[r + 0x3]], ref v[1], ref v[5], ref v[0x9], ref v[0xD]);
            GS_2b(M[Z[r + 0x4]], M[Z[r + 0x5]], ref v[2], ref v[6], ref v[0xA], ref v[0xE]);
            GS_2b(M[Z[r + 0x6]], M[Z[r + 0x7]], ref v[3], ref v[7], ref v[0xB], ref v[0xF]);
            GS_2b(M[Z[r + 0x8]], M[Z[r + 0x9]], ref v[0], ref v[5], ref v[0xA], ref v[0xF]);
            GS_2b(M[Z[r + 0xA]], M[Z[r + 0xB]], ref v[1], ref v[6], ref v[0xB], ref v[0xC]);
            GS_2b(M[Z[r + 0xC]], M[Z[r + 0xD]], ref v[2], ref v[7], ref v[0x8], ref v[0xD]);
            GS_2b(M[Z[r + 0xE]], M[Z[r + 0xF]], ref v[3], ref v[4], ref v[0x9], ref v[0xE]);
        }
        internal static void ROUND_2s(uint r, ref uint[] v, uint[] M)
        {
            GS_2s(M[Z[r     ]], M[Z[r+ 0x1]], ref v[0], ref v[4], ref v[0x8], ref v[0xC]);
		    GS_2s(M[Z[r+ 0x2]], M[Z[r+ 0x3]], ref v[1], ref v[5], ref v[0x9], ref v[0xD]);
		    GS_2s(M[Z[r+ 0x4]], M[Z[r+ 0x5]], ref v[2], ref v[6], ref v[0xA], ref v[0xE]);
		    GS_2s(M[Z[r+ 0x6]], M[Z[r+ 0x7]], ref v[3], ref v[7], ref v[0xB], ref v[0xF]);
		    GS_2s(M[Z[r+ 0x8]], M[Z[r+ 0x9]], ref v[0], ref v[5], ref v[0xA], ref v[0xF]);
		    GS_2s(M[Z[r+ 0xA]], M[Z[r+ 0xB]], ref v[1], ref v[6], ref v[0xB], ref v[0xC]);
		    GS_2s(M[Z[r+ 0xC]], M[Z[r+ 0xD]], ref v[2], ref v[7], ref v[0x8], ref v[0xD]);
		    GS_2s(M[Z[r+ 0xE]], M[Z[r+ 0xF]], ref v[3], ref v[4], ref v[0x9], ref v[0xE]);
        }
        internal static void ProcessBlock(ref BLAKE256_CTX ctx)
        {
            uint i;
            uint[] V = new uint[16];
            Array.Copy(ctx.h, V, 8);
            Array.Copy(ctx.s, 0, V, 8, 4);
            V[0xc] = V[0xd] = (uint)ctx.t << 3;
            V[0xe] = V[0xf] = (uint)(ctx.t >> 29);
            for (i = 8; i < 0x10; i++) V[i] ^= C32[i - 8];
            for (i = 0; i < 14; i++) ROUND((i % 10)<<4, ref V, ctx.buf);
            for (i = 0; i < 8; i++) ctx.h[i] ^= ctx.s[i & 0x3] ^ V[i] ^ V[i + 8];
        }
        internal static void ProcessBlock(ref BLAKE512_CTX ctx)
        {
            uint i;
            ulong[] V = new ulong[16];
            Array.Copy(ctx.h, V, 8);
            Array.Copy(ctx.s, 0, V, 8, 4);
            V[0xc] = V[0xd] = ctx.t0;
            V[0xe] = V[0xf] = ctx.t1;
            for (i = 8; i < 0x10; i++) V[i] ^= C64[i - 8];
            for (i = 0; i < 16; i++) ROUND((i % 10) << 4, ref V, ctx.buf);
            for (i = 0; i < 8; i++) ctx.h[i] ^= ctx.s[i & 0x3] ^ V[i] ^ V[i + 8];
        }
        internal static void ProcessBlock(ref BLAKE2b_CTX ctx, bool last = false)
        {
            uint i;
            ulong[] V = new ulong[16];
            Array.Copy(ctx.h, V, 8);
            Array.Copy(iv512, 0, V, 8, 8);
            V[0xc] ^= ctx.t0;
            V[0xd] ^= ctx.t1;
            if (last) V[0xe] = ~V[0xe];
            for (i = 0; i < 12; i++) ROUND_2b((i % 10) << 4, ref V, ctx.buf);
            for (i = 0; i < 8; i++) ctx.h[i] ^= V[i] ^ V[i + 8];
        }
        internal static void ProcessBlock(ref BLAKE2s_CTX ctx, bool last = false)
        {
            uint i;
            uint[] V = new uint[16];
            Array.Copy(ctx.h, V, 8);
            Array.Copy(iv256, 0, V, 8, 8);
            V[0xc] ^= (uint)ctx.t;
            V[0xd] ^= (uint)(ctx.t >> 32);
            if (last) V[0xe] = ~V[0xe];
            for (i = 0; i < 10; i++) ROUND_2s((i % 10) << 4, ref V, ctx.buf);
            for (i = 0; i < 8; i++) ctx.h[i] ^= V[i] ^ V[i + 8];
        }
        internal static void ProcessBlock(ref BLAKE3_CTX ctx, bool last = false)
        {
            uint i;
            uint[] V = new uint[16];
            //Array.Copy(iv256, V, 8);
            Array.Copy(iv256, 0, V, 8, 8);
            V[0xc] ^= (uint)ctx.t;
            V[0xd] ^= (uint)(ctx.t >> 32);
            V[0xe] ^= (uint)(ctx.t % 64);
            if (last)
            {
                V[0xe] ^= (uint)(ctx.t % 64);
                V[0xf] = ~V[0xf];
            }
            else
            {
                V[0xe] ^= 64;
            }
            for (i = 0; i < 7; i++) ROUND_2s((i % 10) << 4, ref V, ctx.buf);
            for (i = 0; i < 8; i++) ctx.h[i] ^= V[i] ^ V[i + 8];
        }
        internal static BLAKE256_CTX Init256(uint size)
        {
            var ctx = new BLAKE256_CTX();
            ctx.size = size;
            ctx.h = new uint[8];
            ctx.s = new uint[4];
            ctx.buffer = uint_buf.New(16);
            if (size == 256)
            {
                Array.Copy(iv256, ctx.h, 8);
            }
            else
            {
                Array.Copy(iv224, ctx.h, 8);
            }
            return ctx;
        }
        internal static BLAKE512_CTX Init512(uint size)
        {
            var ctx = new BLAKE512_CTX();
            ctx.size = size;
            ctx.h = new ulong[8];
            ctx.s = new ulong[4];
            ctx.buffer = ulong_buf.New(16);
            if (size == 512)
            {
                Array.Copy(iv512, ctx.h, 8);
            }
            else
            {
                Array.Copy(iv384, ctx.h, 8);
            }
            return ctx;
        }
        internal static BLAKE2b_CTX Init2b(uint size)
        {
            var ctx = new BLAKE2b_CTX();
            ctx.size = size;
            ctx.size_key = 0;
            ctx.h = new ulong[8];
            ctx.buffer = ulong_buf_reverse.New(16);
            Array.Copy(iv512, ctx.h, 8);
            ctx.h[0] ^= 0x01010000 ^ (ctx.size_key << 8) ^ (ctx.size >> 3);
            return ctx;
        }
        internal static void InitKey(ref BLAKE2b_CTX ctx, byte[] key, uint size)
        {
            if (size <= 0) return;
            ctx.size_key = size;
            ctx.h[0] = iv512[0] ^ (0x01010000 ^ (ctx.size_key << 8) ^ (ctx.size >> 3));
            var buf = new byte[128];
            Array.Copy(key, buf, (int)size);
            Update(ref ctx, buf, 128);
        }
        internal static BLAKE2s_CTX Init2s(uint size)
        {
            var ctx = new BLAKE2s_CTX();
            ctx.size = size;
            ctx.t = 0;
            ctx.h = new uint[8];
            ctx.buffer = uint_buf_reverse.New(16);
            Array.Copy(iv256, ctx.h, 8);
            ctx.h[0] ^= 0x01010000 ^ (ctx.size_key << 8) ^ (ctx.size >> 3);
            return ctx;
        }
        internal static BLAKE3_CTX Init3(uint size)
        {
            var ctx = new BLAKE3_CTX();
            ctx.size = size;
            ctx.t = 0;
            ctx.h = new uint[8];
            ctx.buffer = uint_buf_reverse.New(16);
            //Array.Copy(iv256, ctx.h, 8);
            ctx.h[0] = 0x01010000 ^ (ctx.size_key << 8) ^ (ctx.size >> 3);
            return ctx;
        }
        internal static void InitKey(ref BLAKE2s_CTX ctx, byte[] key, uint size)
        {
            if (size <= 0) return;
            ctx.size_key = size;
            ctx.h[0] = iv256[0] ^ (0x01010000 ^ (ctx.size_key << 8) ^ (ctx.size >> 3));
            var buf = new byte[64];
            Array.Copy(key, buf, (int)size);
            Update(ref ctx, buf, 128);
        }
        internal static void Update(ref BLAKE256_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ctx.t += 64;
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static void Update(ref BLAKE512_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ctx.t0 += 1024;
                    if (ctx.t0 < 1024) ctx.t1++;
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static void Update(ref BLAKE2b_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ctx.t0 += 128;
                    if (ctx.t0 < 128) ctx.t1++;
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static void Update(ref BLAKE2s_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ctx.t += 64;
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static void Update(ref BLAKE3_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ctx.t += 64;
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static byte[] Final(ref BLAKE256_CTX ctx)
        {
            if (ctx.buffer.IsFULL())
            {
                ctx.t += 64;
                ProcessBlock(ref ctx);
                ctx.t = 0;
            }
            else
            {
                ctx.t = ctx.buffer.Oo.totalbytes;
            }
            if (ctx.buffer.UsedBytes <= 55)
            {
                ctx.buffer.PushNext(0x80);
            }
            else
            {
                ctx.buffer.PushNext(0x80);
                if (ctx.buffer.Used != 0) ctx.buffer.Fill(0, 0);
                ProcessBlock(ref ctx);
                ctx.t = 0;
            }
            ctx.buffer.Fill(0, 2);
            if (ctx.size == 256) ctx.buf[13] |= 1;
            ctx.buffer.PushTotal();
            ProcessBlock(ref ctx);

            var md = new byte[ctx.size >> 3];
            md.CopyFrom_Reverse(0, ctx.h, 0, (int)ctx.size >> 5);
            return md;
        }
        internal static byte[] Final(ref BLAKE512_CTX ctx)
        {
            if (ctx.buffer.IsFULL())
            {
                ctx.t0 += 1024;
                if (ctx.t0 < 1024) ctx.t1++;
                ProcessBlock(ref ctx);
                ctx.t0 = 0;
                ctx.t1 = 0;
            }
            else
            {
                ctx.t0 += (ctx.buffer.Oo.totalbytes_low<<3) % 1024;
                if (ctx.t0 < (ctx.buffer.Oo.totalbytes_low << 3) % 1024) ctx.t1++;
            }
            if (ctx.buffer.UsedBytes <= 111)
            {
                ctx.buffer.PushNext(0x80);
            }
            else
            {
                ctx.buffer.PushNext(0x80);
                if (ctx.buffer.Used != 0) ctx.buffer.Fill(0, 0);
                ProcessBlock(ref ctx);
                ctx.t0 = 0;
                ctx.t1 = 0;
            }
            ctx.buffer.Fill(0, 2);
            if (ctx.size == 512) ctx.buf[13] |= 1;
            ctx.buffer.PushTotal();
            ProcessBlock(ref ctx);

            var md = new byte[ctx.size >> 3];
            int i;
            for (i = 0; i< (ctx.size >> 6); i++)
            {
                md.CopyFrom_Reverse(ctx.h[i], i << 3);
            }
            return md;
        }
        internal static byte[] Final(ref BLAKE2b_CTX ctx)
        {
            if (ctx.buffer.IsFULL())
            {
                ctx.t0 += 128;
                if (ctx.t0 < 128) ctx.t1++;
                ProcessBlock(ref ctx);
            }
            else
            {
                ctx.t0 += (ctx.buffer.Oo.totalbytes_low) % 128;
                if (ctx.t0 < (ctx.buffer.Oo.totalbytes_low) % 128) ctx.t1++;
            }
            ctx.buffer.PushNext(0x00);
            ctx.buffer.Fill(0, 0);
            ProcessBlock(ref ctx, true);

            var md = new byte[ctx.size >> 3];
            int i;
            for (i = 0; i < (ctx.size >> 6); i++)
            {
                md.CopyFrom(ctx.h[i], i << 3);
            }
            if (ctx.size == 160)
            {
                md.CopyFrom(ctx.h[2], 16, 4);
            }
            return md;
        }
        internal static byte[] Final(ref BLAKE2s_CTX ctx)
        {
            if (ctx.buffer.IsFULL())
            {
                ctx.t += 64;
                ProcessBlock(ref ctx);
            }
            else
            {
                ctx.t += (ctx.buffer.Oo.totalbytes) % 64;
            }
            ctx.buffer.PushNext(0x00);
            ctx.buffer.Fill(0, 0);
            ProcessBlock(ref ctx, true);

            var md = new byte[ctx.size >> 3];
            int i;
            for (i = 0; i < (ctx.size >> 5); i++)
            {
                md.CopyFrom(ctx.h[i], i << 2);
            }
            return md;
        }
        internal static byte[] Final(ref BLAKE3_CTX ctx)
        {
            if (ctx.buffer.IsFULL())
            {
                ctx.t += 64;
                ProcessBlock(ref ctx);
            }
            else
            {
                ctx.t += (ctx.buffer.Oo.totalbytes) % 64;
            }
            ctx.buffer.PushNext(0x00);
            ctx.buffer.Fill(0, 0);
            ProcessBlock(ref ctx, true);

            var md = new byte[ctx.size >> 3];
            int i;
            for (i = 0; i < (ctx.size >> 5); i++)
            {
                md.CopyFrom(ctx.h[i], i << 2);
            }
            return md;
        }
    }
    public class BLAKE256 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = BLAKE.Init256(256);
            BLAKE.Update(ref ctx, data, data.Length);
            return BLAKE.Final(ref ctx).ToHexString();
        }
    }
    public class BLAKE224 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = BLAKE.Init256(224);
            BLAKE.Update(ref ctx, data, data.Length);
            return BLAKE.Final(ref ctx).ToHexString();
        }
    }
    public class BLAKE384 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = BLAKE.Init512(384);
            BLAKE.Update(ref ctx, data, data.Length);
            return BLAKE.Final(ref ctx).ToHexString();
        }
    }
    public class BLAKE512 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = BLAKE.Init512(512);
            BLAKE.Update(ref ctx, data, data.Length);
            return BLAKE.Final(ref ctx).ToHexString();
        }
    }
    public class BLAKE2b_160 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = BLAKE.Init2b(160);
            BLAKE.Update(ref ctx, data, data.Length);
            return BLAKE.Final(ref ctx).ToHexString();
        }
    }
    public class BLAKE2b_256 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = BLAKE.Init2b(256);
            BLAKE.Update(ref ctx, data, data.Length);
            return BLAKE.Final(ref ctx).ToHexString();
        }
    }
    public class BLAKE2b_384 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = BLAKE.Init2b(384);
            BLAKE.Update(ref ctx, data, data.Length);
            return BLAKE.Final(ref ctx).ToHexString();
        }
    }
    public class BLAKE2b_512 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = BLAKE.Init2b(512);
            BLAKE.Update(ref ctx, data, data.Length);
            return BLAKE.Final(ref ctx).ToHexString();
        }
    }
    public class BLAKE2s_128 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = BLAKE.Init2s(128);
            BLAKE.Update(ref ctx, data, data.Length);
            return BLAKE.Final(ref ctx).ToHexString();
        }
    }
    public class BLAKE2s_160 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = BLAKE.Init2s(160);
            BLAKE.Update(ref ctx, data, data.Length);
            return BLAKE.Final(ref ctx).ToHexString();
        }
    }
    public class BLAKE2s_224 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = BLAKE.Init2s(224);
            BLAKE.Update(ref ctx, data, data.Length);
            return BLAKE.Final(ref ctx).ToHexString();
        }
    }
    public class BLAKE2s_256 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = BLAKE.Init2s(256);
            BLAKE.Update(ref ctx, data, data.Length);
            return BLAKE.Final(ref ctx).ToHexString();
        }
    }
    //public class BLAKE3_256 : IHash
    //{
    //    public string Make(byte[] data)
    //    {
    //        var ctx = BLAKE.Init3(256);
    //        BLAKE.Update(ref ctx, data, data.Length);
    //        return BLAKE.Final(ref ctx).ToHexString();
    //    }
    //}
}
