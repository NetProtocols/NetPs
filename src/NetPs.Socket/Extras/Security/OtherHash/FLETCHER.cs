namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    internal struct FLETCHER
    {
        internal static FLETCHER16_CTX Init16()
        {
            var ctx = new FLETCHER16_CTX();
            ctx.a = 0;
            ctx.b = 0;
            return ctx;
        }
        internal static FLETCHER32_CTX Init32()
        {
            var ctx = new FLETCHER32_CTX();
            ctx.a = 0;
            ctx.b = 0;
            return ctx;
        }
        internal static FLETCHER64_CTX Init64()
        {
            var ctx = new FLETCHER64_CTX();
            ctx.a = 0;
            ctx.b = 0;
            return ctx;
        }
        internal static void Update(ref FLETCHER16_CTX ctx, byte[] data, int length)
        {
            uint i;
            for (i = 0; i != length; i++)
            {
                ctx.a = (ushort)((ctx.a + data[i]) % byte.MaxValue);
                ctx.b = (ushort)((ctx.b + ctx.a) % byte.MaxValue);
            }
        }
        internal static void Update(ref FLETCHER32_CTX ctx, byte[] data, int length)
        {
            uint i;
            for (i = 0; i != length; i++)
            {
                ctx.a = (ushort)((ctx.a + data[i]) % ushort.MaxValue);
                ctx.b = (ushort)((ctx.b + ctx.a) % ushort.MaxValue);
            }
        }
        internal static void Update(ref FLETCHER64_CTX ctx, byte[] data, int length)
        {
            uint i;
            for (i = 0; i != length; i++)
            {
                ctx.a = (ushort)((ctx.a + data[i]) % uint.MaxValue);
                ctx.b = (ushort)((ctx.b + ctx.a) % uint.MaxValue);
            }
        }
        internal static byte[] Final(ref FLETCHER16_CTX ctx)
        {
            var md = new byte[2];
            md[0] = (byte)ctx.b;
            md[1] = (byte)ctx.a;
            return md;
        }
        internal static byte[] Final(ref FLETCHER32_CTX ctx)
        {
            var md = new byte[sizeof(uint)];
            md.CopyFrom_Reverse(ctx.b << 16 | ctx.a, 0);
            return md;
        }
        internal static byte[] Final(ref FLETCHER64_CTX ctx)
        {
            var md = new byte[sizeof(ulong)];
            md.CopyFrom_Reverse(ctx.b << 32 | ctx.a, 0);
            return md;
        }
    }

    public class FLETCHER16 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = FLETCHER.Init16();
            FLETCHER.Update(ref ctx, data, data.Length);
            return FLETCHER.Final(ref ctx).ToHexString();
        }
    }
    public class FLETCHER32 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = FLETCHER.Init32();
            FLETCHER.Update(ref ctx, data, data.Length);
            return FLETCHER.Final(ref ctx).ToHexString();
        }
    }
    public class FLETCHER64 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = FLETCHER.Init64();
            FLETCHER.Update(ref ctx, data, data.Length);
            return FLETCHER.Final(ref ctx).ToHexString();
        }
    }
}
