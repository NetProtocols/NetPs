namespace NetPs.Socket.Extras.Security.SecureHash
{
    using NetPs.Socket.Memory;
    using System;
    /// <summary>
    /// https://nvlpubs.nist.gov/nistpubs/Legacy/FIPS/NIST.FIPS.180.pdf
    /// </summary>
    public class SHA0
    {
        internal const uint BLOCK_SIZE = 512;
        internal const uint WORD_SIZE = BLOCK_SIZE - 64;
        internal static uint F1(uint b, uint c, uint d)
        {
            return (b & c) | ((~b) & d);
        }
        internal static uint F2(uint b, uint c, uint d)
        {
            return b ^ c ^ d;
        }
        internal static uint F3(uint b, uint c, uint d)
        {
            return (b & c) | (b & d) | (c & d);
        }
        internal delegate uint Func(uint b, uint c, uint d);
        internal static uint S(uint x, byte shift)
        {
            return (x << shift) | (x >> (32 - shift));
        }
        internal static SHA0_CTX Init()
        {
            var c = new SHA0_CTX();
            c.buffer = uint_buf.New(BLOCK_SIZE >> 5);
            c.a = 0x67452301;
            c.b = 0xEFCDAB89;
            c.c = 0x98BADCFE;
            c.d = 0x10325476;
            c.e = 0xC3D2E1F0;
            return c;
        }
        internal static readonly uint[] K = { 0x5A827999, 0x6ED9EBA1, 0x8F1BBCDC, 0xCA62C1D6 };
        internal static readonly Func[] F = { F1, F2, F3, F2 }; 
        internal static void ProcessBlock(ref SHA0_CTX ctx)
        {
            uint i,s, temp;
            uint a, b, c, d, e;
            a = ctx.a;
            b = ctx.b;
            c = ctx.c;
            d = ctx.d;
            e = ctx.e;
            for (i = 0; i < 80; i++)
            {
                s = i & 0xf;
                if (i >= 16)
                {
                    ctx.buf[s] = ctx.buf[(s + 13) & 0xf] ^ ctx.buf[(s + 8) & 0xf] ^ ctx.buf[(s + 2) & 0xf] ^ ctx.buf[s];
                }  
                temp = S(a, 5) + F[i/20](b, c, d) + e + ctx.buf[s]  + K[i/20];
                e = d;
                d = c;
                c = S(b, 30);
                b = a;
                a = temp;
            }
            ctx.a += a;
            ctx.b += b;
            ctx.c += c;
            ctx.d += d;
            ctx.e += e;
        }
        internal static void Update(ref SHA0_CTX c, byte[] data, int length)
        {
            foreach (uint i in c.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref c);
                }
            }
        }
        internal static byte[] Final(ref SHA0_CTX c)
        {
            if (c.buffer.IsFULL()) ProcessBlock(ref c);
            c.buffer.PushNext(0x80);
            if (c.buffer.IsFULL(2)) ProcessBlock(ref c);
            c.buffer.Fill(0, 2);
            c.buffer.PushTotal();
            ProcessBlock(ref c);

            byte[] sha = new byte[20];
            int i = 0;
            sha.CopyFrom_Reverse(c.a, i++ << 2);
            sha.CopyFrom_Reverse(c.b, i++ << 2);
            sha.CopyFrom_Reverse(c.c, i++ << 2);
            sha.CopyFrom_Reverse(c.d, i++ << 2);
            sha.CopyFrom_Reverse(c.e, i++ << 2);
            return sha;
        }
        public string Make(byte[] data)
        {
            var c = Init();
            Update(ref c, data, data.Length);
            var text = Final(ref c).ToHexString();
            return text;
        }
    }
}
