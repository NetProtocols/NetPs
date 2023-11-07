namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    internal class CHECKSUM
    {
        internal static CHECKSUM8_CTX Init8()
        {
            var ctx = new CHECKSUM8_CTX();
            ctx.check = 0;
            return ctx;
        }
        internal static CHECKSUM16_CTX Init16()
        {
            var ctx = new CHECKSUM16_CTX();
            ctx.check = 0;
            ctx.tmp = 0;
            return ctx;
        }
        internal static void Update(ref CHECKSUM8_CTX ctx, byte[] data, int length)
        {
            uint i;
            for (i = 0; i < length; i++) ctx.check += data[i];
        }
        internal static void Update(ref CHECKSUM16_CTX ctx, byte[] data, int length)
        {
            uint i;
            i = 0;
            if (ctx.tmp != 0) ctx.check += (uint)(ctx.tmp << 8) | data[i++];
            ctx.tmp = 0;
            for (; i + 1 < length; i+=2) ctx.check += (uint)(data[i] << 8) | data[i+1];
            if (i != length) ctx.tmp = data[i];
        }
        internal static byte[] Final(ref CHECKSUM8_CTX ctx)
        {
            if (ctx.check > byte.MaxValue) ctx.check = ~ctx.check + 1;
            var md = new byte[1];
            md[0] = (byte)(ctx.check & byte.MaxValue);
            return md;
        }
        internal static byte[] Final(ref CHECKSUM16_CTX ctx)
        {
            ctx.check = (ctx.check >> 16) + (ctx.check & ushort.MaxValue);
            ctx.check = ctx.check + (ctx.check >> 16);
            ctx.check = ~ctx.check;
            var md = new byte[2];
            md[0] = (byte)ctx.check;
            md[1] = (byte)(ctx.check >> 8);
            return md;
        }
    }
    public class CHECKSUM8 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = CHECKSUM.Init8();
            CHECKSUM.Update(ref ctx, data, data.Length);
            return CHECKSUM.Final(ref ctx).ToHexString();
        }
    }
    public class CHECKSUM16 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = CHECKSUM.Init16();
            CHECKSUM.Update(ref ctx, data, data.Length);
            return CHECKSUM.Final(ref ctx).ToHexString();
        }
    }
}
