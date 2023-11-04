namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;

    /// <remarks>
    /// https://ro.uow.edu.au/cgi/viewcontent.cgi?article=2096&context=infopapers
    /// </remarks>
    internal struct HAVAL
    {
        internal static readonly uint[] F2_C = { 0x452821E6, 0x38D01377, 0xBE5466CF, 0x34E90C6C, 0xC0AC29B7, 0xC97C50DD, 0x3F84D5B5, 0xB5470917, 0x9216D5D9, 0x8979FB1B, 0xD1310BA6, 0x98DFB5AC, 0x2FFD72DB, 0xD01ADFB7, 0xB8E1AFED, 0x6A267E96, 0xBA7C9045, 0xF12C7F99, 0x24A19947, 0xB3916CF7, 0x0801F2E2, 0x858EFC16, 0x636920D8, 0x71574E69, 0xA458FEA3, 0xF4933D7E, 0x0D95748F, 0x728EB658, 0x718BCD58, 0x82154AEE, 0x7B54A41D, 0xC25A59B5, };
        internal static readonly byte[] F2_W = { 5 ,14 ,26 ,18 ,11 ,28 ,7 ,16 ,0 ,23 ,20 ,22 ,1 ,10 ,4 ,8 ,30 ,3 ,21 ,9 ,17 ,24 ,29 ,6 ,19 ,12 ,15 ,13 ,2 ,25 ,31 ,27 , }; 
        internal static readonly uint[] F3_C = { 0x9C30D539, 0x2AF26013, 0xC5D1B023, 0x286085F0, 0xCA417918, 0xB8DB38EF, 0x8E79DCB0, 0x603A180E, 0x6C9E0E8B, 0xB01E8A3E, 0xD71577C1, 0xBD314B27, 0x78AF2FDA, 0x55605C60, 0xE65525F3, 0xAA55AB94, 0x57489862, 0x63E81440, 0x55CA396A, 0x2AAB10B6, 0xB4CC5C34, 0x1141E8CE, 0xA15486AF, 0x7C72E993, 0xB3EE1411, 0x636FBC2A, 0x2BA9C55D, 0x741831F6, 0xCE5C3E16, 0x9B87931E, 0xAFD6BA33, 0x6C24CF5C, };
        internal static readonly byte[] F3_W = { 19, 9, 4, 20, 28, 17, 8, 22, 29, 14, 25, 12, 24, 30, 16, 26, 31, 15, 7, 3, 1, 0, 18, 27, 13, 6, 21, 10, 23, 11, 5, 2, };
        internal static readonly uint[] F4_C = { 0x7A325381, 0x28958677, 0x3B8F4898, 0x6B4BB9AF, 0xC4BFE81B, 0x66282193, 0x61D809CC, 0xFB21A991, 0x487CAC60, 0x5DEC8032, 0xEF845D5D, 0xE98575B1, 0xDC262302, 0xEB651B88, 0x23893E81, 0xD396ACC5, 0x0F6D6FF3, 0x83F44239, 0x2E0B4482, 0xA4842004, 0x69C8F04A, 0x9E1F9B5E, 0x21C66842, 0xF6E96C9A, 0x670C9C61, 0xABD388F0, 0x6A51A0D2, 0xD8542F68, 0x960FA728, 0xAB5133A3, 0x6EEF0B6C, 0x137A3BE4, };
        internal static readonly byte[] F4_W = { 24 , 4 , 0 , 14 , 2 , 7 , 28 , 23 , 26 , 6 , 30 , 20 , 18 , 25 , 19 , 3 , 22 , 11 , 31 , 21 , 8 , 27 , 12 , 9 , 1 , 29 , 5 , 15 , 17 , 10 , 16 , 13 , };
        internal static readonly uint[] F5_C = { 0xBA3BF050, 0x7EFB2A98, 0xA1F1651D, 0x39AF0176, 0x66CA593E, 0x82430E88, 0x8CEE8619, 0x456F9FB4, 0x7D84A5C3, 0x3B8B5EBE, 0xE06F75D8, 0x85C12073, 0x401A449F, 0x56C16AA6, 0x4ED3AA62, 0x363F7706, 0x1BFEDF72, 0x429B023D, 0x37D0D724, 0xD00A1248, 0xDB0FEAD3, 0x49F1C09B, 0x075372C9, 0x80991B7B, 0x25D479D8, 0xF6E8DEF7, 0xE3FE501A, 0xB6794C3B, 0x976CE0BD, 0x04C006BA, 0xC1A94FB6, 0x409F60C4, };
        internal static readonly byte[] F5_W = { 27, 3, 21, 26, 17, 11, 20, 29, 19, 0, 12, 7, 13, 8, 31, 10, 5, 9, 14, 30, 18, 6, 28, 24, 2, 23, 16, 22, 4, 1, 25, 15, };
        internal const uint BLOCK_SIZE = 32;
        internal static uint f1(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return ((x5 & (x6 ^ x2)) ^ (x4 & x1) ^ (x3 & x0) ^ x6);
        }
        internal static uint f2(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return ((x4 & ((x5 & ~x3) ^ (x2 & x1) ^ x0 ^ x6)) ^ (x2 & (x5 ^ x1)) ^ (x3 & x1) ^ x6);
        }
        internal static uint f3(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return ((x3 & ((x5 & x4) ^ x0 ^ x6))^ (x5 & x2) ^ (x4 & x1) ^ x6);
        }
        internal static uint f4(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return ((x2 & ((x1 & ~x4) ^ (x3 & ~x0) ^ x5 ^ x0 ^ x6)) ^ (x3 & ((x5 & x4) ^ x1 ^ x0)) ^ (x4 & x0) ^ x6);
        }
        internal static uint f5(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return ((x6 & ((x5 & x4 & x3) ^ ~x1)) ^ (x5 & x2) ^ (x4 & x1) ^ (x3 & x0));
        }
        internal static uint Fphi_1_3(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return f1(x5, x6, x3, x1, x0, x4, x2);
        }
        internal static uint Fphi_1_4(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return f1(x4, x0, x5, x2, x1, x3, x6);
        }
        internal static uint Fphi_1_5(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return f1(x3, x2, x5, x6, x1, x4, x0);
        }
        internal static uint Fphi_2_3(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return f2(x2, x4, x5, x6, x1, x3, x0);
        }
        internal static uint Fphi_2_4(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return f2(x3, x1, x4, x6, x5, x0, x2);
        }
        internal static uint Fphi_2_5(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return f2(x0, x4, x5, x6, x3, x2, x1);
        }
        internal static uint Fphi_3_3(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return f3(x0, x5, x4, x3, x2, x1, x6);
        }
        internal static uint Fphi_3_4(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return f3(x5, x2, x3, x0, x6, x4, x1);
        }
        internal static uint Fphi_3_5(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return f3(x4, x0, x6, x2, x3, x5, x1);
        }
        internal static uint Fphi_4_4(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return f4(x0, x2, x6, x1, x4, x5, x3);
        }
        internal static uint Fphi_4_5(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return f4(x5, x1, x3, x4, x6, x2, x0);
        }
        internal static uint Fphi_5_5(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6)
        {
            return f5(x4, x1, x6, x0, x2, x3, x5);
        }
        internal delegate uint F(uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6);
        internal static uint rotate_right(uint x, byte shift)
        {
            return (x >> shift) | (x << (32 - shift));
        }
        internal static void FF(F f, ref uint x7, uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6, uint w)
        {
            x7 = rotate_right(f(x0, x1, x2, x3, x4, x5, x6), 7) + rotate_right(x7, 11) + w;
        }
        internal static void FFC(F f, ref uint x7, uint x0, uint x1, uint x2, uint x3, uint x4, uint x5, uint x6, uint w, uint c)
        {
            x7 = rotate_right(f(x0, x1, x2, x3, x4, x5, x6), 7) + rotate_right(x7, 11) + w + c;
        }
        internal static void ProcessBlock(ref HAVAL_CTX ctx)
        {
            F f1, f2, f3, f4 = null, f5 = null;

            switch (ctx.passes)
            {
                case 3:
                    f1 = Fphi_1_3;
                    f2 = Fphi_2_3;
                    f3 = Fphi_3_3;
                    break;
                case 4:
                    f1 = Fphi_1_4;
                    f2 = Fphi_2_4;
                    f3 = Fphi_3_4;
                    f4 = Fphi_4_4;
                    break;
                case 5:
                    f1 = Fphi_1_5;
                    f2 = Fphi_2_5;
                    f3 = Fphi_3_5;
                    f4 = Fphi_4_5;
                    f5 = Fphi_5_5;
                    break;
                default:
                    return;
            }

            uint i, j;
            uint[] t = new uint[8];
            Array.Copy(ctx.t, t, 8);
            for (i = 0, j = 0; i < 32; i++, j = i & 0x7)
            {
                FF(f1, ref t[7 - j], t[(14- j)&0x7], t[(13 - j) & 0x7], t[(12 - j) & 0x7], t[(11 - j) & 0x7], t[(10 - j) & 0x7], t[(9 - j) & 0x7], t[(8 - j) & 0x7], ctx.buf[i]);
            }
            for (i = 0, j = 0; i < 32; i++, j = i & 0x7)
            {
                FFC(f2, ref t[7 - j], t[(14 - j) & 0x7], t[(13 - j) & 0x7], t[(12 - j) & 0x7], t[(11 - j) & 0x7], t[(10 - j) & 0x7], t[(9 - j) & 0x7], t[(8 - j) & 0x7], ctx.buf[F2_W[i]], F2_C[i]);
            }
            for (i = 0, j = 0; i < 32; i++, j = i & 0x7)
            {
                FFC(f3, ref t[7 - j], t[(14 - j) & 0x7], t[(13 - j) & 0x7], t[(12 - j) & 0x7], t[(11 - j) & 0x7], t[(10 - j) & 0x7], t[(9 - j) & 0x7], t[(8 - j) & 0x7], ctx.buf[F3_W[i]], F3_C[i]);
            }
            if (f4 == null) goto Record;
            for (i = 0, j = 0; i < 32; i++, j = i & 0x7)
            {
                FFC(f4, ref t[7 - j], t[(14 - j) & 0x7], t[(13 - j) & 0x7], t[(12 - j) & 0x7], t[(11 - j) & 0x7], t[(10 - j) & 0x7], t[(9 - j) & 0x7], t[(8 - j) & 0x7], ctx.buf[F4_W[i]], F4_C[i]);
            }
            if (f5 == null) goto Record;
            for (i = 0, j = 0; i < 32; i++, j = i & 0x7)
            {
                FFC(f5, ref t[7 - j], t[(14 - j) & 0x7], t[(13 - j) & 0x7], t[(12 - j) & 0x7], t[(11 - j) & 0x7], t[(10 - j) & 0x7], t[(9 - j) & 0x7], t[(8 - j) & 0x7], ctx.buf[F5_W[i]], F5_C[i]);
            }

            Record:
            for (i = 0; i < 8; i++)
            {
                ctx.t[i] += t[i];
            }
        }

        internal static HAVAL_CTX Init()
        {
            var ctx = new HAVAL_CTX();
            ctx.passes = 3;
            ctx.size = 256;
            ctx.version = 1;
            ctx.t = new uint[8];
            ctx.t[0] = 0x243F6A88;
            ctx.t[1] = 0x85A308D3;
            ctx.t[2] = 0x13198A2E;
            ctx.t[3] = 0x03707344;
            ctx.t[4] = 0xA4093822;
            ctx.t[5] = 0x299F31D0;
            ctx.t[6] = 0x082EFA98;
            ctx.t[7] = 0xEC4E6C89;
            ctx.buffer = uint_buf_reverse.New(32);
            return ctx;
        }
        internal static void Update(ref HAVAL_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static byte[] Final(ref HAVAL_CTX ctx)
        {
            if (ctx.buffer.IsFULL()) ProcessBlock(ref ctx);
            ctx.buffer.PushNext(0x01);
            if (ctx.buffer.IsFULL(2)) ProcessBlock(ref ctx);
            if ((ctx.buffer.UsedBytes & 0b11) > 2 &&  ctx.buffer.Used == BLOCK_SIZE - 2)
            {
                ctx.buffer.Fill(0, 0);
                ProcessBlock(ref ctx);
            }
            // 10bit
            ctx.buffer.Fill(0, 2);
            ctx.buf[BLOCK_SIZE - 3] |= (uint)(((((ctx.size & 0x3) << 6) | ((ctx.passes & 0x7) << 3) | (ctx.version & 0x7)) & 0xff) << 16);
            ctx.buf[BLOCK_SIZE - 3] |= (uint)(((ctx.size >> 2) & 0xff) << 24);
            ctx.buffer.PushTotal();
            ProcessBlock(ref ctx);

            var md = new byte[ctx.size >> 3];
            switch (ctx.size)
            {
                case 128:
                    ctx.t[0] += rotate_right((ctx.t[7] & 0xff) | (ctx.t[6] & 0xff000000) | (ctx.t[5] & 0xff0000) | (ctx.t[4] & 0xff00), 8);
                    ctx.t[1] += rotate_right((ctx.t[7] & 0xff00) | (ctx.t[6] & 0xff) | (ctx.t[5] & 0xff000000) | (ctx.t[4] & 0xff0000), 16);
                    ctx.t[2] += rotate_right((ctx.t[7] & 0xff0000) | (ctx.t[6] & 0xff00) | (ctx.t[5] & 0xff) | (ctx.t[4] & 0xff000000), 24);
                    ctx.t[3] += (ctx.t[7] & 0xff000000) | (ctx.t[6] & 0xff0000) | (ctx.t[5] & 0xff00) | (ctx.t[4] & 0xff);
                    break;
                case 160:
                    ctx.t[0] += rotate_right((uint)((ctx.t[7] & 0x3F) | (ctx.t[6] & (0x7F << 25)) | (ctx.t[5] & (0x3F << 19))), 19);
                    ctx.t[1] += rotate_right((uint)((ctx.t[7] & (0x3F << 6)) | (ctx.t[6] & 0x3F) | (ctx.t[5] & (0x7F << 25))), 25);
                    ctx.t[2] += (ctx.t[7] & (0x7F << 12)) | (ctx.t[6] & (0x3F << 6)) | (ctx.t[5] & 0x3F);
                    ctx.t[3] += ((ctx.t[7] & (0x3F << 19)) | (ctx.t[6] & (0x7F << 12)) | (ctx.t[5] & (0x3F << 6))) >> 6;
                    ctx.t[4] += (uint)((ulong)((ctx.t[7] & (0x7F << 25)) | (ctx.t[6] & (0x3F << 19)) | (ctx.t[5] & (0x7F << 12))) >> 12);
                    break;
                case 192:
                    ctx.t[0] += rotate_right((uint)((ctx.t[7] & 0x1F) | (ctx.t[6] & (0x3F << 26))), 26);
                    ctx.t[1] += (ctx.t[7] & (0x1F << 5)) | (ctx.t[6] & 0x1F);
                    ctx.t[2] += ((ctx.t[7] & (0x3F << 10)) | (ctx.t[6] & (0x1F << 5))) >> 5;
                    ctx.t[3] += ((ctx.t[7] & (0x1F << 16)) | (ctx.t[6] & (0x3F << 10))) >> 10;
                    ctx.t[4] += ((ctx.t[7] & (0x1F << 21)) | (ctx.t[6] & (0x1F << 16))) >> 16;
                    ctx.t[5] += (uint)((ulong)((ctx.t[7] & (0x3F << 26)) | (ctx.t[6] & (0x1F << 21))) >> 21);
                    break;
                case 224:
                    ctx.t[0] += (ctx.t[7] >> 27) & 0x1F;
                    ctx.t[1] += (ctx.t[7] >> 22) & 0x1F;
                    ctx.t[2] += (ctx.t[7] >> 18) & 0x0F;
                    ctx.t[3] += (ctx.t[7] >> 13) & 0x1F;
                    ctx.t[4] += (ctx.t[7] >> 9) & 0x0F;
                    ctx.t[5] += (ctx.t[7] >> 4) & 0x1F;
                    ctx.t[6] +=  ctx.t[7] & 0x0F;
                    break;
            }
            md.CopyFrom(0, ctx.t, 0, ctx.size >> 5);
            return md;
        }
    }
    public class HAVAL128_3 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(3);
            ctx.SetSize(128);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL160_3 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(3);
            ctx.SetSize(160);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL192_3 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(3);
            ctx.SetSize(192);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL224_3 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(3);
            ctx.SetSize(224);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL256_3 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(3);
            ctx.SetSize(256);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL128_4 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(4);
            ctx.SetSize(128);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL160_4 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(4);
            ctx.SetSize(160);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL192_4 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(4);
            ctx.SetSize(192);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL224_4 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(4);
            ctx.SetSize(224);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL256_4 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(4);
            ctx.SetSize(256);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL128_5 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(5);
            ctx.SetSize(128);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL160_5 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(5);
            ctx.SetSize(160);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL192_5 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(5);
            ctx.SetSize(192);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL224_5 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(5);
            ctx.SetSize(224);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
    public class HAVAL256_5 : IHash
    {
        public string Make(byte[] data)
        {
            var ctx = HAVAL.Init();
            ctx.SetPasses(5);
            ctx.SetSize(256);
            ctx.SetVersion(1);
            HAVAL.Update(ref ctx, data, data.Length);
            return HAVAL.Final(ref ctx).ToHexString();
        }
    }
}
