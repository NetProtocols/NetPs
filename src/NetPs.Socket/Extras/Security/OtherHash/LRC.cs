namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    internal class LRC : IHash
    {
        internal static LRC_CTX Init()
        {
            var ctx = new LRC_CTX();
            ctx.check = 0;
            return ctx;
        }
        internal static void Update(ref LRC_CTX ctx, byte[] data, int length)
        {
            uint i;
            for (i = 0; i < length; i++)
            {
                ctx.check += data[i];
            }
        }
        internal static byte[] Final(ref LRC_CTX ctx)
        {
            var md = new byte[1];
            md[0] = (byte)((ctx.check ^ byte.MaxValue) + 1);
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
