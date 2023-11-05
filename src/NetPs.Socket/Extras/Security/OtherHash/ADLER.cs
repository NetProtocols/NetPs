namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    internal struct ADLER
    {
        internal const uint MOD_ADLER32 = 0xFFF1;
        internal const uint MOD_ADLER64 = 0xFFFFFFFB;
        internal static ADLER32_CTX Init32()
        {
            var ctx = new ADLER32_CTX();
            ctx.a = 1;
            ctx.b = 0;
            return ctx;
        }
        internal static void Update(ref ADLER32_CTX ctx, byte[] data, int length)
        {
            uint i;
            for (i = 0; i != length; i++)
            {
                ctx.a = (ctx.a + data[i]) % MOD_ADLER32;
                ctx.b = (ctx.b + ctx.a) % MOD_ADLER32;
            }
        }
        internal static byte[] Final(ref ADLER32_CTX ctx)
        {
            var md = new byte[sizeof(uint)];
            md.CopyFrom_Reverse(ctx.b << 16 | ctx.a, 0);
            return md;
        }
        internal static ADLER64_CTX Init64()
        {
            var ctx = new ADLER64_CTX();
            ctx.a = 1;
            ctx.b = 0;
            return ctx;
        }
        internal static void Update(ref ADLER64_CTX ctx, byte[] data, int length)
        {
            uint i;
            for (i = 0; i != length; i++)
            {
                ctx.a = (ctx.a + data[i]) % MOD_ADLER64;
                ctx.b = (ctx.b + ctx.a) % MOD_ADLER64;
            }
        }
        internal static byte[] Final(ref ADLER64_CTX ctx)
        {
            var md = new byte[sizeof(ulong)];
            md.CopyFrom_Reverse(ctx.b << 32 | ctx.a, 0);
            return md;
        }
    }

    public class ADLER32 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = ADLER.Init32();
            ADLER.Update(ref ctx, data, data.Length);
            return ADLER.Final(ref ctx).ToHexString();
        }
    }
    public class ADLER64 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = ADLER.Init64();
            ADLER.Update(ref ctx, data, data.Length);
            return ADLER.Final(ref ctx).ToHexString();
        }
    }
}
