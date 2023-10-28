namespace NetPs.Socket.Extras.Security.SecureHash
{
    using NetPs.Socket.Memory;
    using System;
    /// <summary>
    /// https://csrc.nist.gov/files/pubs/fips/202/final/docs/fips_202_draft.pdf
    /// </summary>
    internal struct SHA3
    {
        internal static readonly uint[] Rp = { 0, 1, 190, 28, 91, 36, 300, 6, 55, 276, 3, 10, 171, 153, 231, 105, 45, 15, 21, 136, 210, 66, 253, 120, 78 };
        internal static readonly ulong[] RC = { 0x0000000000000001, 0x0000000000008082, 0x800000000000808a, 0x8000000080008000, 0x000000000000808b, 0x0000000080000001, 0x8000000080008081, 0x8000000000008009, 0x000000000000008a, 0x0000000000000088, 0x0000000080008009, 0x000000008000000a, 0x000000008000808b, 0x800000000000008b, 0x8000000000008089, 0x8000000000008003, 0x8000000000008002, 0x8000000000000080, 0x000000000000800a, 0x800000008000000a, 0x8000000080008081, 0x8000000000008080, 0x0000000080000001, 0x8000000080008008 };
        internal static ulong ROTL(ulong x, byte shift)
        {
            return (x << shift) | (x >> (64 - shift));
        }
        internal static void theta(ref SHA3_CTX ctx)
        {
            uint x, y;
            ulong[] C = new ulong[5], D = new ulong[5];

            for (x = 0; x < 5; x++)
            {
                C[x] = ctx.lane[0][x] ^ ctx.lane[1][x] ^ ctx.lane[2][x] ^ ctx.lane[3][x] ^ ctx.lane[4][x];
            }

            for (x = 0; x < 5; x++)
            {
                D[x] = C[(x + 4) % 5] ^ ROTL(C[(x + 1) % 5], 1);
            }

            for (y = 0; y < 5; y++)
            {
                for(x = 0; x< 5; x++)
                {
                    ctx.lane[y][x] ^= D[x];
                }
            }
        }
        internal static void rho(ref SHA3_CTX ctx)
        {
            uint x, y, m;
            uint t;

            x = 1;
            y = 0;
            for (t = 0; t<24; t++)
            {
                ctx.lane[y][x] = ROTL(ctx.lane[y][x], (byte)( Rp[y*5+x] % 64));
                m = x;
                x = y;
                y = (2 * m + 3 * y) % 5;
            }
        }
        internal static void pi(ref SHA3_CTX ctx)
        {
            ulong[][] Ap = new ulong[5][];
            uint x, y;

            for (y = 0; y<5; y++)
            {
                Ap[y] = new ulong[5];
                for (x = 0; x< 5; x++)
                {
                    Ap[y][x] = ctx.lane[x][(x+3*y) % 5];
                }
            }
            Array.Copy(Ap, ctx.lane, 5);
        }
        internal static void chi(ref SHA3_CTX ctx)
        {
            ulong[][] Ap = new ulong[5][];
            uint x, y;

            for (y = 0; y<5; y++)
            {
                Ap[y] = new ulong[5];
                for (x = 0; x< 5; x++)
                {
                    Ap[y][x] = ctx.lane[y][x] ^ ((~ctx.lane[y][(x+1)%5]) & ctx.lane[y][(x+2)%5]);
                }
            }

            Array.Copy(Ap, ctx.lane, 5);
        }
        internal static void iota(ref SHA3_CTX ctx, uint i)
        {
            ctx.lane[0][0] ^= RC[i];
        }
        internal static void ProcessBlock(ref SHA3_CTX ctx)
        {
            uint t;
            if (ctx.absorbing)
            {
                uint i;
                for (i = 0; i < ctx.b / 8; i++)
                {
                    if (i < ctx.r / 8)
                    {
                        ctx.lane[i / 5][i % 5] ^= ctx.buf[i];
                    }
                }
            }
            for (t = 0; t < ctx.nr; t++)
            {
                theta(ref ctx);
                rho(ref ctx);
                pi(ref ctx);
                chi(ref ctx);
                iota(ref ctx, t);
            }
        }
        internal static SHA3_CTX Init(uint kind, bool shake = false, uint d = 0)
        {
            var ctx = new SHA3_CTX();
            ctx.buffer = ulong_reverse_buf.New(25);
            ctx.b = 200;
            ctx.kind = kind;
            ctx.shake = shake;
            if (ctx.shake)
            {
                if(ctx.kind == 128)
                {
                    if (d == 0) d = 128;
                    ctx.r = 168;
                    ctx.c = 32;
                    ctx.md_size = d / 8;
                }
                else
                {
                    if (d == 0) d = 256;
                    ctx.r = 136;
                    ctx.c = 64;
                    ctx.md_size = d / 8;
                }
            }
            else
            {
                switch (kind)
                {
                    case 224:
                        ctx.r = 144;
                        ctx.c = 56;
                        ctx.md_size = 28;
                        break;
                    case 256:
                        ctx.r = 136;
                        ctx.c = 64;
                        ctx.md_size = 32;
                        break;
                    case 384:
                        ctx.r = 104;
                        ctx.c = 96;
                        ctx.md_size = 48;
                        break;
                    default:
                    case 512:
                        ctx.r = 72;
                        ctx.c = 128;
                        ctx.md_size = 64;
                        break;
                }
            }
            ctx.nr = 24;
            ctx.absorbing = true;
            ctx.lane = new ulong[5][];
            ctx.lane[0] = new ulong[5];
            ctx.lane[1] = new ulong[5];
            ctx.lane[2] = new ulong[5];
            ctx.lane[3] = new ulong[5];
            ctx.lane[4] = new ulong[5];
            return ctx;
        }
        internal static void Update(ref SHA3_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, (int)(200 - ctx.r)))
            {
                if (i < length)
                {
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static byte[] Final(ref SHA3_CTX ctx)
        {
            if (ctx.buffer.UsedBytes <= ctx.r - 2)
            {
                if (ctx.shake)
                {
                    ctx.buffer.SetByte(0x1F, ctx.buffer.UsedBytes);
                }
                else
                {
                    ctx.buffer.SetByte(0x06, ctx.buffer.UsedBytes);
                }
                ctx.buffer.Fill(0, (int)(199 - ctx.r));
                if (ctx.shake)
                {
                    ctx.buffer.SetByte(0x80, (int)(ctx.r - 1));
                }
                else
                {
                    ctx.buffer.SetByte(0x80, (int)(ctx.r - 1));
                }
            }
            else
            {
                if (ctx.shake)
                {
                    ctx.buffer.SetByte(0x9f, ctx.buffer.UsedBytes);
                }
                else
                {
                    ctx.buffer.SetByte(0x86, ctx.buffer.UsedBytes);
                }
            }

            ProcessBlock(ref ctx);
            ctx.absorbing = false;
            var sha = new byte[ctx.md_size];
            if (ctx.md_size <= ctx.r)
            {
                sha.CopyFrom(0, ctx.lane, 0, ctx.md_size);
            }
            else
            {
                sha.CopyFrom(0, ctx.lane, 0, ctx.r);
                var md_size = ctx.r;

                while(md_size < ctx.md_size)
                {
                    ProcessBlock(ref ctx);
                    if (ctx.md_size  -md_size > ctx.r)
                    {
                        sha.CopyFrom((int)md_size >> 3, ctx.lane, 0, ctx.r);
                        md_size += ctx.r;
                    }
                    else
                    {
                        sha.CopyFrom((int)md_size >> 3, ctx.lane, 0, ctx.md_size - md_size);
                        md_size = ctx.md_size;
                    }
                }
            }
            return sha;
        }
    }
    public class SHA3_224
    {
        public string Make(byte[] data)
        {
            var ctx = SHA3.Init(224);
            SHA3.Update(ref ctx, data, data.Length);
            return SHA3.Final(ref ctx).ToHexString();
        }
    }
    public class SHA3_256
    {
        public string Make(byte[] data)
        {
            var ctx = SHA3.Init(256);
            SHA3.Update(ref ctx, data, data.Length);
            return SHA3.Final(ref ctx).ToHexString();
        }
    }
    public class SHA3_384
    {
        public string Make(byte[] data)
        {
            var ctx = SHA3.Init(384);
            SHA3.Update(ref ctx, data, data.Length);
            return SHA3.Final(ref ctx).ToHexString();
        }
    }
    public class SHA3_512
    {
        public string Make(byte[] data)
        {
            var ctx = SHA3.Init(512);
            SHA3.Update(ref ctx, data, data.Length);
            return SHA3.Final(ref ctx).ToHexString();
        }
    }
    public class SHA3_SHAKE128
    {
        public string Make(byte[] data)
        {
            var ctx = SHA3.Init(128, true);
            SHA3.Update(ref ctx, data, data.Length);
            return SHA3.Final(ref ctx).ToHexString();
        }
    }
    public class SHA3_SHAKE256
    {
        public string Make(byte[] data)
        {
            var ctx = SHA3.Init(256, true);
            SHA3.Update(ref ctx, data, data.Length);
            return SHA3.Final(ref ctx).ToHexString();
        }
    }
}
