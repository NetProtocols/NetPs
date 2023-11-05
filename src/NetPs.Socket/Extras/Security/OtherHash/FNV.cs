namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    /// <remarks>
    /// http://www.isthe.com/chongo/tech/comp/fnv/
    /// <br/>
    /// https://datatracker.ietf.org/doc/html/draft-eastlake-fnv-03
    /// </remarks>
    internal struct FNV
    {
        internal static FNV32_CTX Init32()
        {
            var ctx = new FNV32_CTX();
            ctx.hval = 0x811c9dc5;
            return ctx;
        }
        internal static FNV64_CTX Init64()
        {
            var ctx = new FNV64_CTX();
            ctx.hval = 0xcbf29ce484222325;
            return ctx;
        }
        internal static void Update(ref FNV32_CTX ctx, byte[] data, int length)
        {
            uint i = 0;
            if (ctx.mode_a)
            {
                for (i = 0; i < length; i++)
                {
                    ctx.hval ^= data[i];
                    ctx.hval += (ctx.hval << 1) + (ctx.hval << 4) + (ctx.hval << 7) + (ctx.hval << 8) + (ctx.hval << 24);
                }
            }
            else
            {
                for (i = 0; i < length; i++)
                {
                    ctx.hval += (ctx.hval << 1) + (ctx.hval << 4) + (ctx.hval << 7) + (ctx.hval << 8) + (ctx.hval << 24);
                    ctx.hval ^= data[i];
                }
            }
        }
        internal static void Update(ref FNV64_CTX ctx, byte[] data, int length)
        {
            uint i;
            if (ctx.mode_a)
            {
                for (i = 0; i < length; i++)
                {
                    ctx.hval ^= data[i];
                    ctx.hval += (ctx.hval << 1) + (ctx.hval << 4) + (ctx.hval << 5) + (ctx.hval << 7) + (ctx.hval << 8) + (ctx.hval << 40);
                }
            }
            else
            {
                for (i = 0; i < length; i++)
                {
                    ctx.hval += (ctx.hval << 1) + (ctx.hval << 4) + (ctx.hval << 5) + (ctx.hval << 7) + (ctx.hval << 8) + (ctx.hval << 40);
                    ctx.hval ^= data[i];
                }
            }
        }
        internal static byte[] Final(ref FNV32_CTX ctx)
        {
            var md = new byte[sizeof(uint)];
            md.CopyFrom_Reverse(ctx.hval, 0);
            return md;
        }
        internal static byte[] Final(ref FNV64_CTX ctx)
        {
            var md = new byte[sizeof(ulong)];
            md.CopyFrom_Reverse(ctx.hval, 0);
            return md;
        }
    }

    public class FNV1_32 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = FNV.Init32();
            FNV.Update(ref ctx, data, data.Length);
            return FNV.Final(ref ctx).ToHexString();
        }
    }
    public class FNV1_32A : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = FNV.Init32();
            ctx.A_MODE();
            FNV.Update(ref ctx, data, data.Length);
            return FNV.Final(ref ctx).ToHexString();
        }
    }
    public class FNV1_64 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = FNV.Init64();
            FNV.Update(ref ctx, data, data.Length);
            return FNV.Final(ref ctx).ToHexString();
        }
    }
    public class FNV1_64A : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = FNV.Init64();
            ctx.A_MODE();
            FNV.Update(ref ctx, data, data.Length);
            return FNV.Final(ref ctx).ToHexString();
        }
    }
}
