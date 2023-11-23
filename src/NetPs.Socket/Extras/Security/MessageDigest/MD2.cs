namespace NetPs.Socket.Extras.Security.MessageDigest
{
    using System;

    /// <remarks>
    /// https://mirrors.nju.edu.cn/rfc/beta/errata/rfc1319.html
    /// </remarks>
    public class MD2 : IHash
    {
        internal static readonly byte[] S = {
            0x29, 0x2E, 0x43, 0xC9, 0xA2, 0xD8, 0x7C, 0x01, 0x3D, 0x36, 0x54, 0xA1, 0xEC, 0xF0, 0x06, 0x13,
            0x62, 0xA7, 0x05, 0xF3, 0xC0, 0xC7, 0x73, 0x8C, 0x98, 0x93, 0x2B, 0xD9, 0xBC, 0x4C, 0x82, 0xCA,
            0x1E, 0x9B, 0x57, 0x3C, 0xFD, 0xD4, 0xE0, 0x16, 0x67, 0x42, 0x6F, 0x18, 0x8A, 0x17, 0xE5, 0x12,
            0xBE, 0x4E, 0xC4, 0xD6, 0xDA, 0x9E, 0xDE, 0x49, 0xA0, 0xFB, 0xF5, 0x8E, 0xBB, 0x2F, 0xEE, 0x7A,
            0xA9, 0x68, 0x79, 0x91, 0x15, 0xB2, 0x07, 0x3F, 0x94, 0xC2, 0x10, 0x89, 0x0B, 0x22, 0x5F, 0x21,
            0x80, 0x7F, 0x5D, 0x9A, 0x5A, 0x90, 0x32, 0x27, 0x35, 0x3E, 0xCC, 0xE7, 0xBF, 0xF7, 0x97, 0x03,
            0xFF, 0x19, 0x30, 0xB3, 0x48, 0xA5, 0xB5, 0xD1, 0xD7, 0x5E, 0x92, 0x2A, 0xAC, 0x56, 0xAA, 0xC6,
            0x4F, 0xB8, 0x38, 0xD2, 0x96, 0xA4, 0x7D, 0xB6, 0x76, 0xFC, 0x6B, 0xE2, 0x9C, 0x74, 0x04, 0xF1,
            0x45, 0x9D, 0x70, 0x59, 0x64, 0x71, 0x87, 0x20, 0x86, 0x5B, 0xCF, 0x65, 0xE6, 0x2D, 0xA8, 0x02,
            0x1B, 0x60, 0x25, 0xAD, 0xAE, 0xB0, 0xB9, 0xF6, 0x1C, 0x46, 0x61, 0x69, 0x34, 0x40, 0x7E, 0x0F,
            0x55, 0x47, 0xA3, 0x23, 0xDD, 0x51, 0xAF, 0x3A, 0xC3, 0x5C, 0xF9, 0xCE, 0xBA, 0xC5, 0xEA, 0x26,
            0x2C, 0x53, 0x0D, 0x6E, 0x85, 0x28, 0x84, 0x09, 0xD3, 0xDF, 0xCD, 0xF4, 0x41, 0x81, 0x4D, 0x52,
            0x6A, 0xDC, 0x37, 0xC8, 0x6C, 0xC1, 0xAB, 0xFA, 0x24, 0xE1, 0x7B, 0x08, 0x0C, 0xBD, 0xB1, 0x4A,
            0x78, 0x88, 0x95, 0x8B, 0xE3, 0x63, 0xE8, 0x6D, 0xE9, 0xCB, 0xD5, 0xFE, 0x3B, 0x00, 0x1D, 0x39,
            0xF2, 0xEF, 0xB7, 0x0E, 0x66, 0x58, 0xD0, 0xE4, 0xA6, 0x77, 0x72, 0xF8, 0xEB, 0x75, 0x4B, 0x0A,
            0x31, 0x44, 0x50, 0xB4, 0x8F, 0xED, 0x1F, 0x1A, 0xDB, 0x99, 0x8D, 0x33, 0x9F, 0x11, 0x83, 0x14
        };
        internal const int HASH_BLOCK_SIZE = 16;
        internal const int HASH_ROUND_NUM = 18;
        internal const int MD2_CHECKSUM_SIZE = 16;
        internal const int HASH_DIGEST_SIZE = 16;
        internal static void ProcessBlock(ref MD2_CTX ctx)
        {
            uint j, m , k;
            byte[] x = new byte[HASH_BLOCK_SIZE<<1];
            m = ctx.checksum[MD2_CHECKSUM_SIZE - 1];
            for (j = 0; j < HASH_BLOCK_SIZE; j++)
            {
                m = ctx.checksum[j] = (byte)(ctx.checksum[j] ^ S[ctx.buf[j] ^ m]);

                x[j] = ctx.buf[j];
                x[16 + j] = (byte)(ctx.buf[j] ^ ctx.state[j]);
            }

            m = 0;

            for (j = 0; j < HASH_ROUND_NUM; j++)
            {
                for (k = 0; k < HASH_BLOCK_SIZE; k++)       m = ctx.state[k] = (byte)(ctx.state[k] ^ S[m]);
                for (k = 0; k < HASH_BLOCK_SIZE<<1; k++)    m = x[k] = (byte)(x[k] ^ S[m]);
                m = (byte)((m + j) % 256);
            }
        }
        internal static MD2_CTX Init()
        {
            var ctx = new MD2_CTX();
            ctx.buf = new byte[HASH_BLOCK_SIZE];
            ctx.state = new byte[HASH_DIGEST_SIZE];
            ctx.checksum = new byte[MD2_CHECKSUM_SIZE];
            ctx.used = 0;
            return ctx;
        }
        internal static void Update(ref MD2_CTX ctx, byte[] data, int length)
        {
            uint i;
            ctx.total += (uint)length;
            if (ctx.used == HASH_BLOCK_SIZE)
            {
                ProcessBlock(ref ctx);
                ctx.used = 0;
            }

            for (i = 0; i < length; i++)
            {
                ctx.buf[ctx.used] = data[i];
                ctx.used++;
                if (ctx.used == HASH_BLOCK_SIZE && i + 1 < length)
                {
                    ProcessBlock(ref ctx);
                    ctx.used = 0;
                }
            }
        }
        internal static byte[] Final(ref MD2_CTX ctx)
        {
            byte padd;
            if (ctx.used == HASH_BLOCK_SIZE)
            {
                ProcessBlock(ref ctx);
                ctx.used = 0;
            }
            padd = (byte)(HASH_BLOCK_SIZE - ctx.total % HASH_BLOCK_SIZE);
            for (; ctx.used != HASH_BLOCK_SIZE; ctx.used++)
            {
                ctx.buf[ctx.used] = padd;
            }
            ProcessBlock(ref ctx);
            Array.Copy(ctx.checksum, ctx.buf, HASH_BLOCK_SIZE);
            ProcessBlock(ref ctx);

            return ctx.state;
        }
        public virtual string Make(byte[] data)
        {
            var ctx = Init();
            Update(ref ctx, data, data.Length);
            return Final(ref ctx).ToHexString();
        }

        #region S盒 生成
        //public static readonly string PI = "3141592653589793238462643383279502884197169399375105820974944592307816406286208998628034825342117067982148086513282306647093844609550582231725359408128481117450284102701938521105559644622948954930381964428810975665933446128475648233786783165271201909145648566923460348610454326648213393607260249141273724587006606315588174881520920962829254091715364367892590360011330530548820466521384146951941511609433057270365759591953092186117381932611793105118548074462379962749567351885752724891227938183011949129833673362440656643086021394946395224737190702179860943702770539217176293176752384674818467669405132000568127145263560827785771342757789609173637178721468440901224953430146549585371050792279689258923542019956112129021960864034418159813629774771309960518707211349999998372978049951059731732816096318595024459455346908302642522308253344";
        /// <summary>
        /// 生成SBox
        /// </summary>
        /// <param name="PI">PI字符串</param>
        /// <param name="size">SBox 长度</param>
        public static byte[] MakeSBox(string PI, int size =256)
        {
            byte[] S = new byte[size];
            var pi_pos = 0;
            int next_pi()
            {
                return PI[pi_pos++] - 0x30;
            }
            uint rand(uint n)
            {
                int x = next_pi();
                uint y = 10;

                if (n > 10)
                {
                    x = x * 10 + next_pi();
                    y = 100;
                }

                if (n > 100)
                {
                    x = x * 10 + next_pi();
                    y = 1000;
                }

                if (x < (n * (y / n)))
                {
                    return (uint)(x % n);
                }
                else
                {
                    return rand(n);
                }
            }

            uint i;
            uint j;
            uint tmp;

            for (i = 0; i < size; i++)
            {
                S[i] = (byte)i;
            }

            for (i = 2; i < size + 1; i++)
            {
                j = rand(i);
                tmp = S[j];
                S[j] = S[i - 1];
                S[i - 1] = (byte)tmp;
            }
            return S;
        }
        #endregion
    }
}
