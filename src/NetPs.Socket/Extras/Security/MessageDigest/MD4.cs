namespace NetPs.Socket.Extras.Security.MessageDigest
{
    using System;

    ///<remarks>
    ///http://mirrors.nju.edu.cn/rfc/beta/errata/rfc1320.html
    ///</remarks>
    public class MD4
    {
        internal delegate uint RoundFunc(uint x, uint y, uint z);
        internal static uint F(uint x, uint y, uint z)
        {
            return (x & y) | ((~x) & z);
        }
        internal static uint G(uint x, uint y, uint z)
        {
            return (x & y) | (x & z) | (y & z);
        }
        internal static uint H(uint x, uint y, uint z)
        {
            return x ^ y ^ z;
        }
        internal static uint ROTL(uint x, byte shift)
        {
            return (x << shift) | (x >> (32 - shift));
        }
        public const int HASH_BLOCK_SIZE = 64;
        public byte HASH_PADDING_PATTERN = 0x80;
        public const int HASH_LEN_SIZE = 8;
        public const int MD4_BLOCK_SIZE = 64;
        public const int MD4_LEN_SIZE = 8;
        public const int HASH_LEN_OFFSET = (MD4_BLOCK_SIZE - MD4_LEN_SIZE);
        public const int X_SIZE = HASH_BLOCK_SIZE / 4;
        private void PrepareScheduleWord(byte[] block, ref uint[] X, int pos)
        {
            int i, j;
            for (i = 0, j = pos; i < HASH_BLOCK_SIZE / 4; i++, j += 4)
            {
                X[i] = (uint)(block[j] | (block[j + 1] << 8) | (block[j + 2] << 16) | (block[j + 3] << 24));
            }
        }
        private void RecordTotal(ref MD4_CTX info)
        {
            Array.Copy(BitConverter.GetBytes(info.total << 3), 0, info.buf, HASH_LEN_OFFSET, sizeof(long));
        }
        private void ProcessBlock(ref MD4_CTX info, byte[] block, int pos = 0)
        {
            var X = new uint[X_SIZE];
            PrepareScheduleWord(block, ref X, pos);

            var A = info.a;
            var B = info.b;
            var C = info.c;
            var D = info.d;
            uint T;
            RoundFunc Func;
            uint MD4_OP(uint a, uint b, uint c, uint d, int k, byte s)
            {
                return ROTL(a + Func(b, c, d)+ X[k] + T, s);
            }
            T = 0;
            Func = F;
            A = MD4_OP(A, B, C, D, 0, 3);  D=MD4_OP(D, A, B, C, 1, 7);  C=MD4_OP(C, D, A, B, 2, 11);  B=MD4_OP(B, C, D, A, 3, 19);
            A = MD4_OP(A, B, C, D, 4, 3);  D=MD4_OP(D, A, B, C, 5, 7);  C=MD4_OP(C, D, A, B, 6, 11);  B=MD4_OP(B, C, D, A, 7, 19);
            A = MD4_OP(A, B, C, D, 8, 3);  D=MD4_OP(D, A, B, C, 9, 7);  C=MD4_OP(C, D, A, B, 10, 11); B=MD4_OP(B, C, D, A, 11, 19);
            A = MD4_OP(A, B, C, D, 12, 3); D=MD4_OP(D, A, B, C, 13, 7); C=MD4_OP(C, D, A, B, 14, 11); B=MD4_OP(B, C, D, A, 15, 19);
            T = 0x5A827999;
            Func = G;
            A = MD4_OP(A, B, C, D, 0, 3); D=MD4_OP(D, A, B, C, 4, 5); C=MD4_OP(C, D, A, B, 8, 9);  B=MD4_OP(B, C, D, A, 12, 13);
            A = MD4_OP(A, B, C, D, 1, 3); D=MD4_OP(D, A, B, C, 5, 5); C=MD4_OP(C, D, A, B, 9, 9);  B=MD4_OP(B, C, D, A, 13, 13);
            A = MD4_OP(A, B, C, D, 2, 3); D=MD4_OP(D, A, B, C, 6, 5); C=MD4_OP(C, D, A, B, 10, 9); B=MD4_OP(B, C, D, A, 14, 13);
            A = MD4_OP(A, B, C, D, 3, 3); D=MD4_OP(D, A, B, C, 7, 5); C=MD4_OP(C, D, A, B, 11, 9); B=MD4_OP(B, C, D, A, 15, 13);
            T = 0x6ED9EBA1;
            Func = H;
            A = MD4_OP(A, B, C, D, 0, 3); D=MD4_OP(D, A, B, C, 8, 9);  C=MD4_OP(C, D, A, B, 4, 11); B=MD4_OP(B, C, D, A, 12, 15);
            A = MD4_OP(A, B, C, D, 2, 3); D=MD4_OP(D, A, B, C, 10, 9); C=MD4_OP(C, D, A, B, 6, 11); B=MD4_OP(B, C, D, A, 14, 15);
            A = MD4_OP(A, B, C, D, 1, 3); D=MD4_OP(D, A, B, C, 9, 9);  C=MD4_OP(C, D, A, B, 5, 11); B=MD4_OP(B, C, D, A, 13, 15);
            A = MD4_OP(A, B, C, D, 3, 3); D=MD4_OP(D, A, B, C, 11, 9); C=MD4_OP(C, D, A, B, 7, 11); B=MD4_OP(B, C, D, A, 15, 15);
            info.a += A;
            info.b += B;
            info.c += C;
            info.d += D;
        }
        private void Update(ref MD4_CTX c, byte[] data, int len)
        {
            int copy_len = 0;
            int pos = 0;
            if (c.used != 0)
            {
                if (c.used + len < HASH_BLOCK_SIZE)
                {
                    Array.Copy(data, 0, c.buf, c.used, len);
                    c.used += len;
                    return;
                }
                else
                {
                    copy_len = HASH_BLOCK_SIZE - c.used;
                    Array.Copy(data, 0, c.buf, c.used, copy_len);
                    ProcessBlock(ref c, c.buf);

                    c.total += HASH_BLOCK_SIZE;

                    pos += copy_len;
                    len -= copy_len;
                    for (var i = 0; i < HASH_BLOCK_SIZE; i++) c.buf[i] = 0; 
                    c.used = 0;
                }
            }

            if (len < HASH_BLOCK_SIZE)
            {
                Array.Copy(data, 0, c.buf, 0, len);
                c.used += len;
                return;
            }
            else
            {
                var data_n = new uint[data.Length- pos];
                Array.Copy(data, data_n, data_n.Length);
                while (len >= HASH_BLOCK_SIZE)
                {
                    ProcessBlock(ref c, data, pos);
                    c.total += HASH_BLOCK_SIZE;

                    pos += HASH_BLOCK_SIZE;
                    len -= HASH_BLOCK_SIZE;
                }

                Array.Copy(data, pos, c.buf, 0, len);
                c.used = len;
            }
        }
        private byte[] Final(ref MD4_CTX c)
        {
            if (c.used >= (HASH_BLOCK_SIZE - HASH_LEN_SIZE))
            {
                c.total += (uint)c.used;
                c.buf[c.used] = HASH_PADDING_PATTERN;
                c.used++;

                for (var i = c.used; i < HASH_BLOCK_SIZE; i++) c.buf[i] = 0;
                ProcessBlock(ref c, c.buf);

                for (var i = 0; i < HASH_BLOCK_SIZE - HASH_LEN_SIZE; i++) c.buf[i] = 0;
                c.used = 0;

                RecordTotal(ref c);
                ProcessBlock(ref c, c.buf);
            }
            else
            {
                c.total += c.used;
                c.buf[c.used] = HASH_PADDING_PATTERN;
                c.used++;

                for (var i = c.used; i < HASH_BLOCK_SIZE - HASH_LEN_SIZE; i++) c.buf[i] = 0;

                RecordTotal(ref c);
                ProcessBlock(ref c, c.buf);
            }

            var md = new byte[16];
            md.CopyFrom(c.a, 0);
            md.CopyFrom(c.b, 4);
            md.CopyFrom(c.c, 8);
            md.CopyFrom(c.d, 12);
            return md;
        }
        private MD4_CTX Init()
        {
            var c = new MD4_CTX();
            c.a = 0x67452301;
            c.b = 0xEFCDAB89;
            c.c = 0x98BADCFE;
            c.d = 0x10325476;
            c.total = 0;
            c.used = 0;
            c.buf = new byte[64];
            return c;
        }
        public string Make(byte[] data)
        {
            var c = Init();
            Update(ref c, data, data.Length);
            return Final(ref c).ToHexString();
        }
    }
}
