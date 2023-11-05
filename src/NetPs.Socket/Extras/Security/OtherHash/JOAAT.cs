namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    public class JOAAT : IHash
    {
        internal static JOAAT_CTX Init()
        {
            var ctx = new JOAAT_CTX();
            ctx.hash = 0;
            return ctx;
        }
        internal static void Update(ref JOAAT_CTX ctx, byte[] data, int length)
        {
            uint i;
            for (i = 0; i != length; i++)
            {
                ctx.hash += data[i];
                ctx.hash += ctx.hash << 10;
                ctx.hash ^= ctx.hash >> 6;
            }
        }
        internal static byte[] Final(ref JOAAT_CTX ctx)
        {
            ctx.hash += ctx.hash << 3;
            ctx.hash ^= ctx.hash >> 11;
            ctx.hash += ctx.hash << 15;
            var md = new byte[sizeof(uint)];
            md.CopyFrom_Reverse(ctx.hash, 0);
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
