namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    internal class CRC
    {
        internal static byte[] Init_crc_table(CRC8_CTX ctx)
        {
            byte i;
            var table = new byte[256];
            byte j, tmp, castmask, msbmask;

            i = 0; castmask = byte.MaxValue; msbmask = 1 << 7;
            do
            {
                if (ctx.reflected_in) tmp = (byte)(Helper.Bitrev(i) & castmask);
                else tmp = (byte)(i & castmask);
                for (j = 0; j < 8; j++)
                {
                    if ((tmp & msbmask) != 0) tmp = (byte)((tmp << 1) ^ ctx.polynomial);
                    else tmp <<= 1;
                }

                if (ctx.reflected_in) tmp = Helper.Bitrev(tmp);
                table[i] = (byte)(tmp & castmask);

            } while (++i != 0);
            return table;
        }
        internal static ushort[] Init_crc_table(CRC16_CTX ctx)
        {
            byte i;
            var table = new ushort[256];
            ushort j, tmp, castmask, msbmask;

            i = 0; castmask = ushort.MaxValue; msbmask = (ushort)1 << 15;
            do
            {
                if (ctx.reflected_in) tmp = (ushort)(((ushort)Helper.Bitrev(i) << 8) & castmask);
                else tmp = (ushort)(((ushort)i << 8) & castmask);
                for (j = 0; j < 8; j++)
                {
                    if ((tmp & msbmask) != 0) tmp = (ushort)((tmp << 1) ^ ctx.polynomial);
                    else tmp <<= 1;
                }

                if (ctx.reflected_in) tmp = Helper.Bitrev(tmp);
                table[i] = (ushort)(tmp & castmask);

            } while (++i != 0);
            return table;
        }
        internal static uint[] Init_crc_table(CRC32_CTX ctx)
        {
            byte i;
            var table = new uint[256];
            uint j, tmp, castmask, msbmask;

            i = 0; castmask = uint.MaxValue; msbmask = (uint)1 << 31;
            do
            {
                if (ctx.reflected_in) tmp = ((uint)Helper.Bitrev(i) << 24) & castmask;
                else tmp = ((uint)i << 24) & castmask;
                for (j = 0; j < 8; j++)
                {
                    if ((tmp & msbmask) != 0) tmp = (tmp << 1) ^ ctx.polynomial;
                    else tmp <<= 1;
                }

                if (ctx.reflected_in) tmp = Helper.Bitrev(tmp);
                table[i] = tmp & castmask;

            } while (++i != 0);
            return table;
        }
        internal static ulong[] Init_crc_table(CRC64_CTX ctx)
        {
            byte i;
            var table = new ulong[256];
            ulong j, tmp, castmask, msbmask;

            i = 0; castmask = ulong.MaxValue; msbmask = (ulong)1 << 63;
            do
            {
                if (ctx.reflected_in) tmp = ((ulong)Helper.Bitrev(i) << 56) & castmask;
                else tmp = ((ulong)i << 56) & castmask;
                for (j = 0; j < 8; j++)
                {
                    if ((tmp & msbmask) != 0) tmp = (tmp << 1) ^ ctx.polynomial;
                    else tmp <<= 1;
                }

                if (ctx.reflected_in) tmp = Helper.Bitrev(tmp);
                table[i] = tmp & castmask;

            } while (++i != 0);
            return table;
        }
        internal static CRC8_CTX Init8()
        {
            var ctx = new CRC8_CTX();
            ctx.reflected_in = false;
            ctx.reflected_out = false;
            ctx.polynomial = 0;
            ctx.xor = 0;
            return ctx;
        }
        internal static CRC16_CTX Init16()
        {
            var ctx = new CRC16_CTX();
            ctx.reflected_in = false;
            ctx.reflected_out = false;
            ctx.polynomial = 0;
            ctx.xor = 0;
            return ctx;
        }
        internal static CRC32_CTX Init32()
        {
            var ctx = new CRC32_CTX();
            ctx.reflected_in = false;
            ctx.reflected_out = false;
            ctx.polynomial = 0;
            ctx.xor = 0;
            return ctx;
        }
        internal static CRC64_CTX Init64()
        {
            var ctx = new CRC64_CTX();
            ctx.reflected_in = false;
            ctx.reflected_out = false;
            ctx.polynomial = 0;
            ctx.xor = 0;
            return ctx;
        }
        internal static void Update(ref CRC8_CTX ctx, byte[] data, int length)
        {
            uint i;
            if (ctx.reflected_out)
            {
                for (i = 0; i != length; i++) ctx.crc = (byte)(ctx.crc_table[(ctx.crc ^ data[i]) & 0xff] ^ (ctx.crc >> 8));
            }
            else
            {
                for (i = 0; i != length; i++) ctx.crc = (byte)(ctx.crc_table[(ctx.crc ^ data[i]) & 0xff] ^ (ctx.crc << 8));
            }
        }
        internal static void Update(ref CRC16_CTX ctx, byte[] data, int length)
        {
            uint i;
            if (ctx.reflected_out)
            {
                for (i = 0; i != length; i++) ctx.crc = (ushort)(ctx.crc_table[(ctx.crc ^ data[i]) & 0xff] ^ (ctx.crc >> 8));
            }
            else
            {
                for (i = 0; i != length; i++) ctx.crc = (ushort)(ctx.crc_table[((ctx.crc >> 8) ^ data[i]) & 0xff] ^ (ctx.crc << 8));
            }
        }
        internal static void Update(ref CRC32_CTX ctx, byte[] data, int length)
        {
            uint i;
            if (ctx.reflected_out)
            {
                for (i = 0; i != length; i++) ctx.crc = ctx.crc_table[(ctx.crc ^ data[i]) & 0xff] ^ (ctx.crc >> 8);
            }
            else
            {
                for (i = 0; i != length; i++) ctx.crc = ctx.crc_table[((ctx.crc >> 24) ^ data[i]) & 0xff] ^ (ctx.crc << 8);
            }
        }
        internal static void Update(ref CRC64_CTX ctx, byte[] data, int length)
        {
            uint i;
            if (ctx.reflected_out)
            {
                for (i = 0; i != length; i++) ctx.crc = ctx.crc_table[(ctx.crc ^ data[i]) & 0xff] ^ (ctx.crc >> 8);
            }
            else
            {
                for (i = 0; i != length; i++) ctx.crc = ctx.crc_table[((ctx.crc >> 56) ^ data[i]) & 0xff] ^ (ctx.crc << 8);
            }
        }
        internal static byte[] Final(ref CRC8_CTX ctx)
        {
            ctx.crc = (byte)(ctx.crc ^ ctx.xor);
            var md = new byte[1];
            md[0] = ctx.crc;
            return md;
        }
        internal static byte[] Final(ref CRC16_CTX ctx)
        {
            ctx.crc = (ushort)(ctx.crc ^ ctx.xor);
            var md = new byte[2];
            md[0] = (byte)(ctx.crc >> 8);
            md[1] = (byte)(ctx.crc);
            return md;
        }
        internal static byte[] Final(ref CRC32_CTX ctx)
        {
            ctx.crc = ctx.crc ^ ctx.xor;
            var md = new byte[sizeof(uint)];
            md.CopyFrom_Reverse(ctx.crc, 0);
            return md;
        }
        internal static byte[] Final(ref CRC64_CTX ctx)
        {
            ctx.crc = ctx.crc ^ ctx.xor;
            var md = new byte[sizeof(ulong)];
            md.CopyFrom_Reverse(ctx.crc, 0);
            return md;
        }
    }
    public class CRC8_CUSTOM
    {
        private byte[] crc_table { get; set; }
        protected byte poly { get; set; }
        protected byte initval { get; set; }
        protected byte xor { get; set; }
        protected bool reflectIn { get; set; }
        protected bool reflectOut { get; set; }
        public CRC8_CUSTOM(byte poly, byte initval, byte xor, bool reflectIn, bool reflectOut)
        {
            this.poly = poly;
            this.initval = initval;
            this.xor = xor;
            this.reflectIn = reflectIn;
            this.reflectOut = reflectOut;
        }
        public string Make(byte[] data)
        {
            var ctx = CRC.Init8();
            ctx.SetInitValue(initval);
            ctx.SetXor(xor);
            if (reflectOut) ctx.SetReflectedOut();
            if (crc_table == null)
            {
                ctx.SetPolynomial(this.poly);
                if (reflectIn) ctx.SetReflectedIn();
                ctx.crc_table = CRC.Init_crc_table(ctx);
            }
            CRC.Update(ref ctx, data, data.Length);
            return CRC.Final(ref ctx).ToHexString();
        }
    }
    public class CRC16_CUSTOM
    {
        private ushort[] crc_table { get; set; }
        protected short poly { get; set; }
        protected short initval { get; set; }
        protected short xor { get; set; }
        protected bool reflectIn { get; set; }
        protected bool reflectOut { get; set; }
        public CRC16_CUSTOM(short poly, short initval, short xor, bool reflectIn, bool reflectOut)
        {
            this.poly = poly;
            this.initval = initval;
            this.xor = xor;
            this.reflectIn = reflectIn;
            this.reflectOut = reflectOut;
        }
        public string Make(byte[] data)
        {
            var ctx = CRC.Init16();
            ctx.SetInitValue(initval);
            ctx.SetXor(xor);
            if (reflectOut) ctx.SetReflectedOut();
            if (crc_table == null)
            {
                ctx.SetPolynomial(this.poly);
                if (reflectIn) ctx.SetReflectedIn();
                ctx.crc_table = CRC.Init_crc_table(ctx);
            }
            CRC.Update(ref ctx, data, data.Length);
            return CRC.Final(ref ctx).ToHexString();
        }
    }
    public class CRC32_CUSTOM
    {
        private uint[] crc_table { get; set; }
        protected int poly { get; set; }
        protected int initval { get; set; }
        protected int xor { get; set; }
        protected bool reflectIn { get; set; }
        protected bool reflectOut { get; set; }
        public CRC32_CUSTOM(int poly, int initval, int xor, bool reflectIn, bool reflectOut)
        {
            this.poly = poly;
            this.initval = initval;
            this.xor = xor;
            this.reflectIn = reflectIn;
            this.reflectOut = reflectOut;
        }
        public string Make(byte[] data)
        {
            var ctx = CRC.Init32();
            ctx.SetInitValue(initval);
            ctx.SetXor(xor);
            if (reflectOut) ctx.SetReflectedOut();
            if (crc_table == null)
            {
                ctx.SetPolynomial(this.poly);
                if (reflectIn) ctx.SetReflectedIn();
                ctx.crc_table = CRC.Init_crc_table(ctx);
            }
            CRC.Update(ref ctx, data, data.Length);
            return CRC.Final(ref ctx).ToHexString();
        }
    }
    public class CRC64_CUSTOM
    {
        private ulong[] crc_table { get; set; }
        protected long poly { get; set; }
        protected long initval { get; set; }
        protected long xor { get; set; }
        protected bool reflectIn { get; set; }
        protected bool reflectOut { get; set; }
        public CRC64_CUSTOM(long poly, long initval, long xor, bool reflectIn, bool reflectOut)
        {
            this.poly = poly;
            this.initval = initval;
            this.xor = xor;
            this.reflectIn = reflectIn;
            this.reflectOut = reflectOut;
        }
        public string Make(byte[] data)
        {
            var ctx = CRC.Init64();
            ctx.SetInitValue(initval);
            ctx.SetXor(xor);
            if (reflectOut) ctx.SetReflectedOut();
            if (crc_table == null)
            {
                ctx.SetPolynomial(this.poly);
                if (reflectIn) ctx.SetReflectedIn();
                ctx.crc_table = CRC.Init_crc_table(ctx);
            }
            CRC.Update(ref ctx, data, data.Length);
            return CRC.Final(ref ctx).ToHexString();
        }
    }
    public class CRC8 : CRC8_CUSTOM, IHash
    {
        public CRC8() : base(0x07, 0, 0, false, false) { }
    }
    public class CRC8_SAE_J1850 : CRC8_CUSTOM, IHash
    {
        public CRC8_SAE_J1850() : base(0x1D, 0xFF, 0xFF, false, false) { }
    }
    public class CRC8_SAE_J1850_ZERO : CRC8_CUSTOM, IHash
    {
        public CRC8_SAE_J1850_ZERO() : base(0x1D, 0x00, 0x00, false, false) { }
    }
    public class CRC8_8H2F : CRC8_CUSTOM, IHash
    {
        public CRC8_8H2F() : base(0x2F, 0xFF, 0xFF, false, false) { }
    }
    public class CRC8_CDMA2000 : CRC8_CUSTOM, IHash
    {
        public CRC8_CDMA2000() : base(0x9B, 0xFF, 0x00, false, false) { }
    }
    public class CRC8_DARC : CRC8_CUSTOM, IHash
    {
        public CRC8_DARC() : base(0x39, 0x00, 0x00, true, true) { }
    }
    public class CRC8_DVB_S2 : CRC8_CUSTOM, IHash
    {
        public CRC8_DVB_S2() : base(0xD5, 0x00, 0x00, false, false) { }
    }
    public class CRC8_EBU : CRC8_CUSTOM, IHash
    {
        public CRC8_EBU() : base(0x1D, 0xFF, 0x00, true, true) { }
    }
    public class CRC8_ICODE : CRC8_CUSTOM, IHash
    {
        public CRC8_ICODE() : base(0x1D, 0xFD, 0x00, false, false) { }
    }
    public class CRC8_ITU : CRC8_CUSTOM, IHash
    {
        public CRC8_ITU() : base(0x07, 0x00, 0x55, false, false) { }
    }
    public class CRC8_MAXIM : CRC8_CUSTOM, IHash
    {
        public CRC8_MAXIM() : base(0x31, 0x00, 0x00, true, true) { }
    }
    public class CRC8_ROHC : CRC8_CUSTOM, IHash
    {
        public CRC8_ROHC() : base(0x07, 0xFF, 0x00, true, true) { }
    }
    public class CRC8_WCDMA : CRC8_CUSTOM, IHash
    {
        public CRC8_WCDMA() : base(0x9B, 0x00, 0x00, true, true) { }
    }
    public class CRC16_CCIT_ZERO : CRC16_CUSTOM, IHash
    {
        public CRC16_CCIT_ZERO() : base(0x1021, 0x0000, 0x0000, false, false) { }
    }
    public class CRC16_ARC : CRC16_CUSTOM, IHash
    {
        //0x8005
        public CRC16_ARC() : base(-32763, 0x0000, 0x0000, true, true) { }
    }
    public class CRC16_AUG_CCITT : CRC16_CUSTOM, IHash
    {
        public CRC16_AUG_CCITT() : base(0x1021, 0x1D0F, 0x0000, false, false) { }
    }
    public class CRC16_BUYPASS : CRC16_CUSTOM, IHash
    {
        //0x8005
        public CRC16_BUYPASS() : base(-32763, 0x0000, 0x0000, false, false) { }
    }
    public class CRC16_CCITT_FALSE : CRC16_CUSTOM, IHash
    {
        public CRC16_CCITT_FALSE() : base(0x1021, -1, 0x0000, false, false) { }
    }
    public class CRC16_CDMA2000 : CRC16_CUSTOM, IHash
    {
        //0xC867
        public CRC16_CDMA2000() : base(-14233, -1, 0x0000, false, false) { }
    }
    public class CRC16_DDS_110 : CRC16_CUSTOM, IHash
    {
        public CRC16_DDS_110() : base(-32763, -32755, 0x0000, false, false) { }
    }
    public class CRC16_DECT_R : CRC16_CUSTOM, IHash
    {
        public CRC16_DECT_R() : base(0x0589, 0x0000, 0x0001, false, false) { }
    }
    public class CRC16_DECT_X : CRC16_CUSTOM, IHash
    {
        public CRC16_DECT_X() : base(0x0589, 0x0000, 0x0000, false, false) { }
    }
    public class CRC16_DNP : CRC16_CUSTOM, IHash
    {
        public CRC16_DNP() : base(0x3D65, 0x0000, -1, true, true) { }
    }
    public class CRC16_EN_13757 : CRC16_CUSTOM, IHash
    {
        public CRC16_EN_13757() : base(0x3D65, 0x0000, -1, false, false) { }
    }
    public class CRC16_GENIBUS : CRC16_CUSTOM, IHash
    {
        public CRC16_GENIBUS() : base(0x1021, -1, -1, false, false) { }
    }
    public class CRC16_MAXIM : CRC16_CUSTOM, IHash
    {
        public CRC16_MAXIM() : base(-32763, 0, -1, true, true) { }
    }
    public class CRC16_MCRF4XX : CRC16_CUSTOM, IHash
    {
        public CRC16_MCRF4XX() : base(0x1021, -1, 0, true, true) { }
    }
    public class CRC16_RIELLO : CRC16_CUSTOM, IHash
    {
        public CRC16_RIELLO() : base(0x1021, -19798, 0, true, true) { }
    }
    public class CRC16_T10_DIF : CRC16_CUSTOM, IHash
    {
        public CRC16_T10_DIF() : base(-29769, 0, 0, false, false) { }
    }
    public class CRC16_TELEDISK : CRC16_CUSTOM, IHash
    {
        public CRC16_TELEDISK() : base(-24425, 0, 0, false, false) { }
    }
    public class CRC16_TMS37157 : CRC16_CUSTOM, IHash
    {
        public CRC16_TMS37157() : base(0x1021, -30228, 0, true, true) { }
    }
    public class CRC16_USB : CRC16_CUSTOM, IHash
    {
        public CRC16_USB() : base(-32763, -1, -1, true, true) { }
    }
    public class CRC16_A : CRC16_CUSTOM, IHash
    {
        public CRC16_A() : base(0x1021, -14650, 0, true, true) { }
    }
    public class CRC16_KERMIT : CRC16_CUSTOM, IHash
    {
        public CRC16_KERMIT() : base(0x1021, 0, 0, true, true) { }
    }
    public class CRC16_MODBUS : CRC16_CUSTOM, IHash
    {
        public CRC16_MODBUS() : base(-32763, -1, 0, true, true) { }
    }
    public class CRC16_X_25 : CRC16_CUSTOM, IHash
    {
        public CRC16_X_25() : base(0x1021, -1, -1, true, true) { }
    }
    public class CRC16_XMODEM : CRC16_CUSTOM, IHash
    {
        public CRC16_XMODEM() : base(0x1021, 0, 0, false, false) { }
    }
    public class CRC32 : CRC32_CUSTOM, IHash
    {
        public CRC32() : base(0x4C11DB7, -1, -1, true, true) { }
    }
    public class CRC32_BZIP2 : CRC32_CUSTOM, IHash
    {
        public CRC32_BZIP2() : base(0x4C11DB7, -1, -1, false, false) { }
    }
    public class CRC32_C : CRC32_CUSTOM, IHash
    {
        public CRC32_C() : base(0x1EDC6F41, -1, -1, true, true) { }
    }
    public class CRC32_D : CRC32_CUSTOM, IHash
    {
        //0xA833982B
        public CRC32_D() : base(-1473013717, -1, -1, true, true) { }
    }
    public class CRC32_MPEG2 : CRC32_CUSTOM, IHash
    {
        public CRC32_MPEG2() : base(0x04C11DB7, -1, 0, false, false) { }
    }
    public class CRC32_POSIX : CRC32_CUSTOM, IHash
    {
        public CRC32_POSIX() : base(0x04C11DB7, 0, -1, false, false) { }
    }
    public class CRC32_Q : CRC32_CUSTOM, IHash
    {
        //0x814141AB
        public CRC32_Q() : base(-2126429781, 0, 0, false, false) { }
    }
    public class CRC32_JAMCRC : CRC32_CUSTOM, IHash
    {
        public CRC32_JAMCRC() : base(0x04C11DB7, -1, 0, true, true) { }
    }
    public class CRC32_XFER : CRC32_CUSTOM, IHash
    {
        public CRC32_XFER() : base(0x000000AF, 0, 0, false, false) { }
    }
    public class CRC64_ECMA_182 : CRC64_CUSTOM, IHash
    {
        public CRC64_ECMA_182() : base(0x42f0e1eba9ea3693, 0, 0, false, false) { }
    }
    public class CRC64_GO_ISO : CRC64_CUSTOM, IHash
    {
        public CRC64_GO_ISO() : base(0x000000000000001B, -1, -1, true, true) { }
    }
    public class CRC64_WE : CRC64_CUSTOM, IHash
    {
        public CRC64_WE() : base(0x42f0e1eba9ea3693, -1, -1, false, false) { }
    }
    public class CRC64_XZ : CRC64_CUSTOM, IHash
    {
        public CRC64_XZ() : base(0x42f0e1eba9ea3693, -1, -1, true, true) { }
    }
}
