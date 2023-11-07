namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    internal class BCC : IHash
    {
        internal static BCC_CTX Init()
        {
            var ctx = new BCC_CTX();
            ctx.check = 0;
            return ctx;
        }
        internal static void Update(ref BCC_CTX ctx, byte[] data, int length)
        {
            uint i;
            for (i = 0; i < length; i++)
            {
                ctx.check ^= data[i];
            }
        }
        internal static byte[] Final(ref BCC_CTX ctx)
        {
            var md = new byte[1];
            md[0] = ctx.check;
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
