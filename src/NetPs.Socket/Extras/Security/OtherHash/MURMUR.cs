namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;
    internal class MURMUR
    {
        internal static uint ROTL32(uint x, byte shift)
        {
            return (x << shift) | (x >> (32 - shift));
        }
        internal static ulong ROTL64(ulong x, byte shift)
        {
            return (x << shift) | (x >> (64 - shift));
        }
        internal static uint fmix32(uint h)
        {
            h = (h ^ (h >> 16)) * 0x85ebca6b;
            h = (h ^ (h >> 13)) * 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }
        internal static ulong fmix64(ulong h)
        {
            h = (h ^ (h >> 33)) * 0xff51afd7ed558ccd;
            h = (h ^ (h >> 33)) * 0xc4ceb9fe1a85ec53;
            h ^= h >> 33;
            return h;
        }
        internal const uint c1 = 0x239b961b;
        internal const uint c2 = 0xab0e9789;
        internal const uint c3 = 0x38b34ae5;
        internal const uint c4 = 0xa1e38b93;
        internal const uint c1_32 = 0xcc9e2d51;
        internal const uint c2_32 = 0x1b873593;
        internal const ulong c1_x64 = 0x87c37b91114253d5;
        internal const ulong c2_x64 = 0x4cf5ad432745937f;
        internal static void ProcessBlock(ref MURMUR_X86_128_CTX ctx)
        {
            ctx.h1 ^= ROTL32(ctx.buf[0] * c1, 15) * c2;
            ctx.h1  = (ROTL32(ctx.h1, 19) + ctx.h2) * 5 + 0x561ccd1b;
            ctx.h2 ^= ROTL32(ctx.buf[1] * c2, 16) * c3;
            ctx.h2  = (ROTL32(ctx.h2, 17) + ctx.h3) * 5 + 0x0bcaa747;
            ctx.h3 ^= ROTL32(ctx.buf[2] * c3, 17) * c4;
            ctx.h3  = (ROTL32(ctx.h3, 15) + ctx.h4) * 5 + 0x96cd1c35;
            ctx.h4 ^= ROTL32(ctx.buf[3] * c4, 18) * c1;
            ctx.h4  = (ROTL32(ctx.h4, 13) + ctx.h1) * 5 + 0x32ac3b17;
        }
        internal static void ProcessBlock(ref MURMUR_X86_32_CTX ctx)
        {
            ctx.h1 ^= ROTL32(ctx.buf[0] * c1_32, 15) * c2_32;
            ctx.h1  = ROTL32(ctx.h1, 13) * 5 + 0xe6546b64;
        }
        internal static void ProcessBlock(ref MURMUR_X64_128_CTX ctx)
        {
            ctx.h1 ^= ROTL64(ctx.buf[0] * c1_x64, 31) * c2_x64;
            ctx.h1  = (ROTL64(ctx.h1, 27) + ctx.h2) * 5 + 0x52dce729;
            ctx.h2 ^= ROTL64(ctx.buf[1] * c2_x64, 33) * c1_x64;
            ctx.h2  = (ROTL64(ctx.h2, 31) + ctx.h1) * 5 + 0x38495ab5;
        }
        internal static MURMUR_X86_128_CTX Init_X86_128()
        {
            var ctx = new MURMUR_X86_128_CTX();
            ctx.SetSeed(0);
            ctx.buffer = uint_buf_reverse.New(4);
            return ctx;
        }
        internal static MURMUR_X86_32_CTX Init_X86_32()
        {
            var ctx = new MURMUR_X86_32_CTX();
            ctx.SetSeed(0);
            ctx.buffer = uint_buf_reverse.New(1);
            return ctx;
        }
        internal static MURMUR_X64_128_CTX Init_X64_128()
        {
            var ctx = new MURMUR_X64_128_CTX();
            ctx.SetSeed(0);
            ctx.buffer = ulong_buf_reverse.New(2);
            return ctx;
        }
        internal static void Update(ref MURMUR_X86_128_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static void Update(ref MURMUR_X86_32_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static void Update(ref MURMUR_X64_128_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static byte[] Final(ref MURMUR_X86_128_CTX ctx)
        {
            if (ctx.buffer.IsFULL()) ProcessBlock(ref ctx);
            else if (ctx.buffer.UsedBytes != 0)
            {
                do
                {
                    ctx.h1 ^= ROTL32(ctx.buf[0] * c1, 15) * c2;
                    if (ctx.buffer.UsedBytes < 5) break;
                    ctx.h2 ^= ROTL32(ctx.buf[1] * c2, 16) * c3;
                    if (ctx.buffer.UsedBytes < 9) break;
                    ctx.h3 ^= ROTL32(ctx.buf[2] * c3, 17) * c4;
                    if (ctx.buffer.UsedBytes < 13) break;
                    ctx.h4 ^= ROTL32(ctx.buf[3] * c4, 18) * c1;
                    break;
                } while (true);
            }
            ctx.h1 ^= (uint)ctx.buffer.Oo.totalbytes;
            ctx.h2 ^= (uint)ctx.buffer.Oo.totalbytes;
            ctx.h3 ^= (uint)ctx.buffer.Oo.totalbytes;
            ctx.h4 ^= (uint)ctx.buffer.Oo.totalbytes;

            ctx.h1 += ctx.h2;
            ctx.h1 += ctx.h3;
            ctx.h1 += ctx.h4;
            ctx.h2 += ctx.h1;
            ctx.h3 += ctx.h1;
            ctx.h4 += ctx.h1;

            ctx.h1 = fmix32(ctx.h1);
            ctx.h2 = fmix32(ctx.h2);
            ctx.h3 = fmix32(ctx.h3);
            ctx.h4 = fmix32(ctx.h4);

            ctx.h1 += ctx.h2;
            ctx.h1 += ctx.h3;
            ctx.h1 += ctx.h4;
            ctx.h2 += ctx.h1;
            ctx.h3 += ctx.h1;
            ctx.h4 += ctx.h1;

            byte[] md = new byte[16];
            int i = 0;
            md.CopyFrom_Reverse(ctx.h1, i++ << 2);
            md.CopyFrom_Reverse(ctx.h2, i++ << 2);
            md.CopyFrom_Reverse(ctx.h3, i++ << 2);
            md.CopyFrom_Reverse(ctx.h4, i++ << 2);
            return md;
        }
        internal static byte[] Final(ref MURMUR_X86_32_CTX ctx)
        {
            if (ctx.buffer.IsFULL()) ProcessBlock(ref ctx);
            else if (ctx.buffer.UsedBytes != 0)
            {
                ctx.h1 ^= ROTL32(ctx.buf[0] * c1_32, 15) * c2_32;
            }

            ctx.h1 ^= (uint)ctx.buffer.Oo.totalbytes;
            ctx.h1 = fmix32(ctx.h1);

            byte[] md = new byte[4];
            md.CopyFrom_Reverse(ctx.h1, 0);
            return md;
        }
        internal static byte[] Final(ref MURMUR_X64_128_CTX ctx)
        {
            if (ctx.buffer.IsFULL()) ProcessBlock(ref ctx);
            else if (ctx.buffer.UsedBytes != 0)
            {
                do
                {
                    ctx.h1 ^= ROTL64(ctx.buf[0] * c1_x64, 31) * c2_x64;
                    if (ctx.buffer.UsedBytes < 9) break;
                    ctx.h2 ^= ROTL64(ctx.buf[1] * c2_x64, 33) * c1_x64;
                    break;
                } while (true);
            }

            ctx.h1 ^= ctx.buffer.Oo.totalbytes_low;
            ctx.h2 ^= ctx.buffer.Oo.totalbytes_low;

            ctx.h1 += ctx.h2;
            ctx.h2 += ctx.h1;

            ctx.h1 = fmix64(ctx.h1);
            ctx.h2 = fmix64(ctx.h2);

            ctx.h1 += ctx.h2;
            ctx.h2 += ctx.h1;

            byte[] md = new byte[16];
            int i = 0;
            md.CopyFrom_Reverse2(ctx.h1, i++ << 3);
            md.CopyFrom_Reverse2(ctx.h2, i++ << 3);
            return md;
        }
    }

    public class MURMUR_X86_128 : IHash
    {
        public static int DEFAILT_SEED = 123;
        public int Seed { get; set; }
        public MURMUR_X86_128() { this.Seed = DEFAILT_SEED; }
        public MURMUR_X86_128(int seed) { this.Seed = seed; }
        public string Make(byte[] data)
        {
            var ctx = MURMUR.Init_X86_128();
            ctx.SetSeed(Seed);
            MURMUR.Update(ref ctx, data, data.Length);
            return MURMUR.Final(ref ctx).ToHexString();
        }
    }
    public class MURMUR_X86_32 : IHash
    {
        public static int DEFAILT_SEED = 1234;
        public int Seed { get; set; }
        public MURMUR_X86_32() { this.Seed = DEFAILT_SEED; }
        public MURMUR_X86_32(int seed) { this.Seed = seed; }
        public string Make(byte[] data)
        {
            var ctx = MURMUR.Init_X86_32();
            ctx.SetSeed(Seed);
            MURMUR.Update(ref ctx, data, data.Length);
            return MURMUR.Final(ref ctx).ToHexString();
        }
    }
    public class MURMUR_X64_128 : IHash
    {
        public static long DEFAILT_SEED = 123;
        public long Seed { get; set; }
        public MURMUR_X64_128() { this.Seed = DEFAILT_SEED; }
        public MURMUR_X64_128(long seed) { this.Seed = seed; }
        public string Make(byte[] data)
        {
            var ctx = MURMUR.Init_X64_128();
            ctx.SetSeed(Seed);
            MURMUR.Update(ref ctx, data, data.Length);
            return MURMUR.Final(ref ctx).ToHexString();
        }
    }
}
