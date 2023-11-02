namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;

    /// <remarks>
    /// https://homes.esat.kuleuven.be/~bosselae/ripemd160/pdf/AB-9601/AB-9601.pdf
    /// </remarks>
    internal class RIPEMD
    {
        internal static uint ROL(uint x, byte shift)
        {
            return (x << shift) | (x >> (32 - shift));
        }
        internal static uint F(uint x, uint y, uint z)
        {
            return x ^ y ^ z;
        }
        internal static uint G(uint x, uint y, uint z)
        {
            return (x & y) | (~x & z);
        }
        internal static uint H(uint x, uint y, uint z)
        {
            return (x | ~y) ^ z;
        }
        internal static uint IQ(uint x, uint y, uint z)
        {
            return (x & z) | (y & ~z);
        }
        internal static uint J(uint x, uint y, uint z)
        {
            return x ^ (y | ~z);
        }
        private delegate uint FN(uint x, uint y, uint z);
        internal static readonly uint[] HH1 = { 0, 0x5a827999, 0x6ed9eba1, 0x8f1bbcdc, 0x50a28be6, 0x5c4dd124, 0x6d703ef3, 0 };
        internal static readonly uint[] HH2 = { 0, 0x5a827999, 0x6ed9eba1, 0x8f1bbcdc, 0xa953fd4e, 0x50a28be6, 0x5c4dd124, 0x6d703ef3, 0x7a6d76e9, 0 };
        
        internal static void ProcessBlock(ref RIPEMD_CTX ctx)
        {
            uint a, b, c, d, e, aa, bb, cc, dd, ee, temp;
            FN _f;
            uint[] HH;
            byte hh;
            uint[] buf = ctx.buf;
            a = ctx.a; b = ctx.b; c = ctx.c; d = ctx.d; e = ctx.e;
            switch (ctx.size)
            {
                case 128:
                    HH = HH1;
                    aa = a; bb = b; cc = c; dd = d;
                    uint op128(uint @a, uint @b, uint @c, uint @d, byte i, byte j)
                    {
                        return ROL(@a + _f(@b, @c, @d) + buf[i] + HH[hh], j);
                    }
                    _f = F;
                    hh = 0;
                    a = op128(a, b, c, d, 0, 11); d = op128(d, a, b, c, 1, 14); c = op128(c, d, a, b, 2, 15); b = op128(b, c, d, a, 3, 12);
                    a = op128(a, b, c, d, 4, 5); d = op128(d, a, b, c, 5, 8); c = op128(c, d, a, b, 6, 7); b = op128(b, c, d, a, 7, 9);
                    a = op128(a, b, c, d, 8, 11); d = op128(d, a, b, c, 9, 13); c = op128(c, d, a, b, 10, 14); b = op128(b, c, d, a, 11, 15);
                    a = op128(a, b, c, d, 12, 6); d = op128(d, a, b, c, 13, 7); c = op128(c, d, a, b, 14, 9); b = op128(b, c, d, a, 15, 8);
                    _f = G;
                    hh++;
                    a = op128(a, b, c, d, 7, 7);d = op128(d, a, b, c, 4, 6);c = op128(c, d, a, b, 13, 8);b = op128(b, c, d, a, 1, 13);
                    a = op128(a, b, c, d, 10, 11); d = op128(d, a, b, c, 6, 9); c = op128(c, d, a, b, 15, 7); b = op128(b, c, d, a, 3, 15);
                    a = op128(a, b, c, d, 12, 7); d = op128(d, a, b, c, 0, 12); c = op128(c, d, a, b, 9, 15); b = op128(b, c, d, a, 5, 9);
                    a = op128(a, b, c, d, 2, 11); d = op128(d, a, b, c, 14, 7); c = op128(c, d, a, b, 11, 13); b = op128(b, c, d, a, 8, 12);
                    _f = H;
                    hh++;
                    a = op128(a, b, c, d, 3, 11); d = op128(d, a, b, c, 10, 13); c = op128(c, d, a, b, 14, 6); b = op128(b, c, d, a, 4, 7);
                    a = op128(a, b, c, d, 9, 14); d = op128(d, a, b, c, 15, 9); c = op128(c, d, a, b, 8, 13); b = op128(b, c, d, a, 1, 15);
                    a = op128(a, b, c, d, 2, 14); d = op128(d, a, b, c, 7, 8); c = op128(c, d, a, b, 0, 13); b = op128(b, c, d, a, 6, 6);
                    a = op128(a, b, c, d, 13, 5); d = op128(d, a, b, c, 11, 12); c = op128(c, d, a, b, 5, 7); b = op128(b, c, d, a, 12, 5);
                    _f = IQ;
                    hh++;
                    a = op128(a, b, c, d, 1, 11); d = op128(d, a, b, c, 9, 12); c = op128(c, d, a, b, 11, 14); b = op128(b, c, d, a, 10, 15);
                    a = op128(a, b, c, d, 0, 14); d = op128(d, a, b, c, 8, 15); c = op128(c, d, a, b, 12, 9); b = op128(b, c, d, a, 4, 8);
                    a = op128(a, b, c, d, 13, 9); d = op128(d, a, b, c, 3, 14); c = op128(c, d, a, b, 7, 5); b = op128(b, c, d, a, 15, 6);
                    a = op128(a, b, c, d, 14, 8); d = op128(d, a, b, c, 5, 6); c = op128(c, d, a, b, 6, 5); b = op128(b, c, d, a, 2, 12);
                    _f = IQ;
                    hh++;
                    aa = op128(aa, bb, cc, dd, 5, 8); dd = op128(dd, aa, bb, cc, 14, 9); cc = op128(cc, dd, aa, bb, 7, 9); bb = op128(bb, cc, dd, aa, 0, 11);
                    aa = op128(aa, bb, cc, dd, 9, 13); dd = op128(dd, aa, bb, cc, 2, 15); cc = op128(cc, dd, aa, bb, 11, 15); bb = op128(bb, cc, dd, aa, 4, 5);
                    aa = op128(aa, bb, cc, dd, 13, 7); dd = op128(dd, aa, bb, cc, 6, 7); cc = op128(cc, dd, aa, bb, 15, 8); bb = op128(bb, cc, dd, aa, 8, 11);
                    aa = op128(aa, bb, cc, dd, 1, 14); dd = op128(dd, aa, bb, cc, 10, 14); cc = op128(cc, dd, aa, bb, 3, 12); bb = op128(bb, cc, dd, aa, 12, 6);
                    _f = H;
                    hh++;
                    aa = op128(aa, bb, cc, dd, 6, 9); dd = op128(dd, aa, bb, cc, 11, 13); cc = op128(cc, dd, aa, bb, 3, 15); bb = op128(bb, cc, dd, aa, 7, 7);
                    aa = op128(aa, bb, cc, dd, 0, 12); dd = op128(dd, aa, bb, cc, 13, 8); cc = op128(cc, dd, aa, bb, 5, 9); bb = op128(bb, cc, dd, aa, 10, 11);
                    aa = op128(aa, bb, cc, dd, 14, 7); dd = op128(dd, aa, bb, cc, 15, 7); cc = op128(cc, dd, aa, bb, 8, 12); bb = op128(bb, cc, dd, aa, 12, 7);
                    aa = op128(aa, bb, cc, dd, 4, 6); dd = op128(dd, aa, bb, cc, 9, 15); cc = op128(cc, dd, aa, bb, 1, 13); bb = op128(bb, cc, dd, aa, 2, 11);
                    _f = G;
                    hh++;
                    aa = op128(aa, bb, cc, dd, 15, 9); dd = op128(dd, aa, bb, cc, 5, 7); cc = op128(cc, dd, aa, bb, 1, 15); bb = op128(bb, cc, dd, aa, 3, 11);
                    aa = op128(aa, bb, cc, dd, 7, 8); dd = op128(dd, aa, bb, cc, 14, 6); cc = op128(cc, dd, aa, bb, 6, 6); bb = op128(bb, cc, dd, aa, 9, 14);
                    aa = op128(aa, bb, cc, dd, 11, 12); dd = op128(dd, aa, bb, cc, 8, 13); cc = op128(cc, dd, aa, bb, 12, 5); bb = op128(bb, cc, dd, aa, 2, 14);
                    aa = op128(aa, bb, cc, dd, 10, 13); dd = op128(dd, aa, bb, cc, 0, 13); cc = op128(cc, dd, aa, bb, 4, 7); bb = op128(bb, cc, dd, aa, 13, 5);
                    _f = F;
                    hh++;
                    aa = op128(aa, bb, cc, dd, 8, 15); dd = op128(dd, aa, bb, cc, 6, 5); cc = op128(cc, dd, aa, bb, 4, 8); bb = op128(bb, cc, dd, aa, 1, 11);
                    aa = op128(aa, bb, cc, dd, 3, 14); dd = op128(dd, aa, bb, cc, 11, 14); cc = op128(cc, dd, aa, bb, 15, 6); bb = op128(bb, cc, dd, aa, 0, 14);
                    aa = op128(aa, bb, cc, dd, 5, 6); dd = op128(dd, aa, bb, cc, 12, 9); cc = op128(cc, dd, aa, bb, 2, 12); bb = op128(bb, cc, dd, aa, 13, 9);
                    aa = op128(aa, bb, cc, dd, 9, 12); dd = op128(dd, aa, bb, cc, 7, 5); cc = op128(cc, dd, aa, bb, 10, 15); bb = op128(bb, cc, dd, aa, 14, 8);

                    temp = ctx.a;
                    ctx.a = ctx.b + c + dd;
                    ctx.b = ctx.c + d + aa;
                    ctx.c = ctx.d + a + bb;
                    ctx.d = temp + b + cc;
                    break;
                case 160:
                    HH = HH2;
                    aa = a; bb = b; cc = c; dd = d; ee = e;
                    void op160(ref uint @a, uint @b, ref uint @c, uint @d, uint @e, byte i, byte j)
                    {
                        @a += _f(@b, @c, @d) + buf[i] + HH[hh];
                        @a = ROL(@a, j) + @e;
                        @c = ROL(@c, 10);
                    }

                    _f = F;
                    hh = 0;
                    op160(ref a, b, ref c, d, e, 0, 11); op160(ref e, a, ref b, c, d, 1, 14); op160(ref d, e, ref a, b, c, 2, 15); op160(ref c, d, ref e, a, b, 3, 12);
                    op160(ref b, c, ref d, e, a, 4, 5); op160(ref a, b, ref c, d, e, 5, 8); op160(ref e, a, ref b, c, d, 6, 7); op160(ref d, e, ref a, b, c, 7, 9);
                    op160(ref c, d, ref e, a, b, 8, 11); op160(ref b, c, ref d, e, a, 9, 13); op160(ref a, b, ref c, d, e, 10, 14); op160(ref e, a, ref b, c, d, 11, 15);
                    op160(ref d, e, ref a, b, c, 12, 6); op160(ref c, d, ref e, a, b, 13, 7); op160(ref b, c, ref d, e, a, 14, 9); op160(ref a, b, ref c, d, e, 15, 8);
                    _f = G;
                    hh ++;
                    op160(ref e, a, ref b, c, d, 7, 7); op160(ref d, e, ref a, b, c, 4, 6); op160(ref c, d, ref e, a, b, 13, 8); op160(ref b, c, ref d, e, a, 1, 13);
                    op160(ref a, b, ref c, d, e, 10, 11); op160(ref e, a, ref b, c, d, 6, 9); op160(ref d, e, ref a, b, c, 15, 7); op160(ref c, d, ref e, a, b, 3, 15);
                    op160(ref b, c, ref d, e, a, 12, 7); op160(ref a, b, ref c, d, e, 0, 12); op160(ref e, a, ref b, c, d, 9, 15); op160(ref d, e, ref a, b, c, 5, 9);
                    op160(ref c, d, ref e, a, b, 2, 11); op160(ref b, c, ref d, e, a, 14, 7); op160(ref a, b, ref c, d, e, 11, 13); op160(ref e, a, ref b, c, d, 8, 12);
                    _f = H;
                    hh ++;
                    op160(ref d, e, ref a, b, c, 3, 11); op160(ref c, d, ref e, a, b, 10, 13); op160(ref b, c, ref d, e, a, 14, 6); op160(ref a, b, ref c, d, e, 4, 7);
                    op160(ref e, a, ref b, c, d, 9, 14); op160(ref d, e, ref a, b, c, 15, 9); op160(ref c, d, ref e, a, b, 8, 13); op160(ref b, c, ref d, e, a, 1, 15);
                    op160(ref a, b, ref c, d, e, 2, 14); op160(ref e, a, ref b, c, d, 7, 8); op160(ref d, e, ref a, b, c, 0, 13); op160(ref c, d, ref e, a, b, 6, 6);
                    op160(ref b, c, ref d, e, a, 13, 5); op160(ref a, b, ref c, d, e, 11, 12); op160(ref e, a, ref b, c, d, 5, 7); op160(ref d, e, ref a, b, c, 12, 5);
                    _f = IQ;
                    hh ++;
                    op160(ref c, d, ref e, a, b, 1, 11); op160(ref b, c, ref d, e, a, 9, 12); op160(ref a, b, ref c, d, e, 11, 14); op160(ref e, a, ref b, c, d, 10, 15);
                    op160(ref d, e, ref a, b, c, 0, 14); op160(ref c, d, ref e, a, b, 8, 15); op160(ref b, c, ref d, e, a, 12, 9); op160(ref a, b, ref c, d, e, 4, 8);
                    op160(ref e, a, ref b, c, d, 13, 9); op160(ref d, e, ref a, b, c, 3, 14); op160(ref c, d, ref e, a, b, 7, 5); op160(ref b, c, ref d, e, a, 15, 6);
                    op160(ref a, b, ref c, d, e, 14, 8); op160(ref e, a, ref b, c, d, 5, 6); op160(ref d, e, ref a, b, c, 6, 5); op160(ref c, d, ref e, a, b, 2, 12);
                    _f = J;
                    hh++;
                    op160(ref b, c, ref d, e, a, 4, 9); op160(ref a, b, ref c, d, e, 0, 15); op160(ref e, a, ref b, c, d, 5, 5); op160(ref d, e, ref a, b, c, 9, 11);
                    op160(ref c, d, ref e, a, b, 7, 6); op160(ref b, c, ref d, e, a, 12, 8); op160(ref a, b, ref c, d, e, 2, 13); op160(ref e, a, ref b, c, d, 10, 12);
                    op160(ref d, e, ref a, b, c, 14, 5); op160(ref c, d, ref e, a, b, 1, 12); op160(ref b, c, ref d, e, a, 3, 13); op160(ref a, b, ref c, d, e, 8, 14);
                    op160(ref e, a, ref b, c, d, 11, 11); op160(ref d, e, ref a, b, c, 6, 8); op160(ref c, d, ref e, a, b, 15, 5); op160(ref b, c, ref d, e, a, 13, 6);

                    _f = J;
                    hh++;
                    op160(ref aa, bb, ref cc, dd, ee, 5, 8); op160(ref ee, aa, ref bb, cc, dd, 14, 9); op160(ref dd, ee, ref aa, bb, cc, 7, 9); op160(ref cc, dd, ref ee, aa, bb, 0, 11);
                    op160(ref bb, cc, ref dd, ee, aa, 9, 13); op160(ref aa, bb, ref cc, dd, ee, 2, 15); op160(ref ee, aa, ref bb, cc, dd, 11, 15); op160(ref dd, ee, ref aa, bb, cc, 4, 5);
                    op160(ref cc, dd, ref ee, aa, bb, 13, 7); op160(ref bb, cc, ref dd, ee, aa, 6, 7); op160(ref aa, bb, ref cc, dd, ee, 15, 8); op160(ref ee, aa, ref bb, cc, dd, 8, 11);
                    op160(ref dd, ee, ref aa, bb, cc, 1, 14); op160(ref cc, dd, ref ee, aa, bb, 10, 14); op160(ref bb, cc, ref dd, ee, aa, 3, 12); op160(ref aa, bb, ref cc, dd, ee, 12, 6);
                    _f = IQ;
                    hh++;
                    op160(ref ee, aa, ref bb, cc, dd, 6, 9); op160(ref dd, ee, ref aa, bb, cc, 11, 13); op160(ref cc, dd, ref ee, aa, bb, 3, 15); op160(ref bb, cc, ref dd, ee, aa, 7, 7);
                    op160(ref aa, bb, ref cc, dd, ee, 0, 12); op160(ref ee, aa, ref bb, cc, dd, 13, 8); op160(ref dd, ee, ref aa, bb, cc, 5, 9); op160(ref cc, dd, ref ee, aa, bb, 10, 11);
                    op160(ref bb, cc, ref dd, ee, aa, 14, 7); op160(ref aa, bb, ref cc, dd, ee, 15, 7); op160(ref ee, aa, ref bb, cc, dd, 8, 12); op160(ref dd, ee, ref aa, bb, cc, 12, 7);
                    op160(ref cc, dd, ref ee, aa, bb, 4, 6); op160(ref bb, cc, ref dd, ee, aa, 9, 15); op160(ref aa, bb, ref cc, dd, ee, 1, 13); op160(ref ee, aa, ref bb, cc, dd, 2, 11);
                    _f = H;
                    hh++;
                    op160(ref dd, ee, ref aa, bb, cc, 15, 9); op160(ref cc, dd, ref ee, aa, bb, 5, 7); op160(ref bb, cc, ref dd, ee, aa, 1, 15); op160(ref aa, bb, ref cc, dd, ee, 3, 11);
                    op160(ref ee, aa, ref bb, cc, dd, 7, 8); op160(ref dd, ee, ref aa, bb, cc, 14, 6); op160(ref cc, dd, ref ee, aa, bb, 6, 6); op160(ref bb, cc, ref dd, ee, aa, 9, 14);
                    op160(ref aa, bb, ref cc, dd, ee, 11, 12); op160(ref ee, aa, ref bb, cc, dd, 8, 13); op160(ref dd, ee, ref aa, bb, cc, 12, 5); op160(ref cc, dd, ref ee, aa, bb, 2, 14);
                    op160(ref bb, cc, ref dd, ee, aa, 10, 13); op160(ref aa, bb, ref cc, dd, ee, 0, 13); op160(ref ee, aa, ref bb, cc, dd, 4, 7); op160(ref dd, ee, ref aa, bb, cc, 13, 5);
                    _f = G;
                    hh++;
                    op160(ref cc, dd, ref ee, aa, bb, 8, 15); op160(ref bb, cc, ref dd, ee, aa, 6, 5); op160(ref aa, bb, ref cc, dd, ee, 4, 8); op160(ref ee, aa, ref bb, cc, dd, 1, 11);
                    op160(ref dd, ee, ref aa, bb, cc, 3, 14); op160(ref cc, dd, ref ee, aa, bb, 11, 14); op160(ref bb, cc, ref dd, ee, aa, 15, 6); op160(ref aa, bb, ref cc, dd, ee, 0, 14);
                    op160(ref ee, aa, ref bb, cc, dd, 5, 6); op160(ref dd, ee, ref aa, bb, cc, 12, 9); op160(ref cc, dd, ref ee, aa, bb, 2, 12); op160(ref bb, cc, ref dd, ee, aa, 13, 9);
                    op160(ref aa, bb, ref cc, dd, ee, 9, 12); op160(ref ee, aa, ref bb, cc, dd, 7, 5); op160(ref dd, ee, ref aa, bb, cc, 10, 15); op160(ref cc, dd, ref ee, aa, bb, 14, 8);
                    _f = F;
                    hh++;
                    op160(ref bb, cc, ref dd, ee, aa, 12, 8); op160(ref aa, bb, ref cc, dd, ee, 15, 5); op160(ref ee, aa, ref bb, cc, dd, 10, 12); op160(ref dd, ee, ref aa, bb, cc, 4, 9);
                    op160(ref cc, dd, ref ee, aa, bb, 1, 12); op160(ref bb, cc, ref dd, ee, aa, 5, 5); op160(ref aa, bb, ref cc, dd, ee, 8, 14); op160(ref ee, aa, ref bb, cc, dd, 7, 6);
                    op160(ref dd, ee, ref aa, bb, cc, 6, 8); op160(ref cc, dd, ref ee, aa, bb, 2, 13); op160(ref bb, cc, ref dd, ee, aa, 13, 6); op160(ref aa, bb, ref cc, dd, ee, 14, 5);
                    op160(ref ee, aa, ref bb, cc, dd, 0, 15); op160(ref dd, ee, ref aa, bb, cc, 3, 13); op160(ref cc, dd, ref ee, aa, bb, 9, 11); op160(ref bb, cc, ref dd, ee, aa, 11, 11);

                    temp = ctx.b + c + dd;
                    ctx.b = ctx.c + d + ee;
                    ctx.c = ctx.d + e + aa;
                    ctx.d = ctx.e + a + bb;
                    ctx.e = ctx.a + b + cc;
                    ctx.a = temp;
                    break;
                case 256:
                    HH = HH1;
                    byte h1 = 0, h2 = 4;
                    aa = ctx.aa; bb = ctx.bb; cc = ctx.cc; dd = ctx.dd;
                    uint op256(uint @a, uint @b, uint @c, uint @d, byte x, byte shift)
                    {
                        return ROL(@a + _f(@b, @c, @d) + buf[x] + HH[hh], shift);
                    }
                    _f = F;
                    hh = h1++;
                    a = op256(a, b, c, d, 0, 11); d = op256(d, a, b, c, 1, 14); c = op256(c, d, a, b, 2, 15); b = op256(b, c, d, a, 3, 12);
                    a = op256(a, b, c, d, 4, 5); d = op256(d, a, b, c, 5, 8); c = op256(c, d, a, b, 6, 7); b = op256(b, c, d, a, 7, 9);
                    a = op256(a, b, c, d, 8, 11); d = op256(d, a, b, c, 9, 13); c = op256(c, d, a, b, 10, 14); b = op256(b, c, d, a, 11, 15);
                    a = op256(a, b, c, d, 12, 6); d = op256(d, a, b, c, 13, 7); c = op256(c, d, a, b, 14, 9); b = op256(b, c, d, a, 15, 8);

                    hh = h2++;
                    _f = IQ;
                    aa = op256(aa, bb, cc, dd, 5, 8); dd = op256(dd, aa, bb, cc, 14, 9); cc = op256(cc, dd, aa, bb, 7, 9); bb = op256(bb, cc, dd, aa, 0, 11);
                    aa = op256(aa, bb, cc, dd, 9, 13); dd = op256(dd, aa, bb, cc, 2, 15); cc = op256(cc, dd, aa, bb, 11, 15); bb = op256(bb, cc, dd, aa, 4, 5);
                    aa = op256(aa, bb, cc, dd, 13, 7); dd = op256(dd, aa, bb, cc, 6, 7); cc = op256(cc, dd, aa, bb, 15, 8); bb = op256(bb, cc, dd, aa, 8, 11);
                    aa = op256(aa, bb, cc, dd, 1, 14); dd = op256(dd, aa, bb, cc, 10, 14); cc = op256(cc, dd, aa, bb, 3, 12); bb = op256(bb, cc, dd, aa, 12, 6);
                    temp = a; a = aa; aa = temp;

                    _f = G;
                    hh = h1++;
                    a = op256(a, b, c, d, 7, 7); d = op256(d, a, b, c, 4, 6); c = op256(c, d, a, b, 13, 8); b = op256(b, c, d, a, 1, 13);
                    a = op256(a, b, c, d, 10, 11); d = op256(d, a, b, c, 6, 9); c = op256(c, d, a, b, 15, 7); b = op256(b, c, d, a, 3, 15);
                    a = op256(a, b, c, d, 12, 7); d = op256(d, a, b, c, 0, 12); c = op256(c, d, a, b, 9, 15); b = op256(b, c, d, a, 5, 9);
                    a = op256(a, b, c, d, 2, 11); d = op256(d, a, b, c, 14, 7); c = op256(c, d, a, b, 11, 13); b = op256(b, c, d, a, 8, 12);
                    hh = h2++;
                    _f = H;
                    aa = op256(aa, bb, cc, dd, 6, 9); dd = op256(dd, aa, bb, cc, 11, 13); cc = op256(cc, dd, aa, bb, 3, 15); bb = op256(bb, cc, dd, aa, 7, 7);
                    aa = op256(aa, bb, cc, dd, 0, 12); dd = op256(dd, aa, bb, cc, 13, 8); cc = op256(cc, dd, aa, bb, 5, 9); bb = op256(bb, cc, dd, aa, 10, 11);
                    aa = op256(aa, bb, cc, dd, 14, 7); dd = op256(dd, aa, bb, cc, 15, 7); cc = op256(cc, dd, aa, bb, 8, 12); bb = op256(bb, cc, dd, aa, 12, 7);
                    aa = op256(aa, bb, cc, dd, 4, 6); dd = op256(dd, aa, bb, cc, 9, 15); cc = op256(cc, dd, aa, bb, 1, 13); bb = op256(bb, cc, dd, aa, 2, 11);
                    temp = b; b = bb; bb = temp;

                    _f = H;
                    hh = h1++;
                    a = op256(a, b, c, d, 3, 11); d = op256(d, a, b, c, 10, 13); c = op256(c, d, a, b, 14, 6); b = op256(b, c, d, a, 4, 7);
                    a = op256(a, b, c, d, 9, 14); d = op256(d, a, b, c, 15, 9); c = op256(c, d, a, b, 8, 13); b = op256(b, c, d, a, 1, 15);
                    a = op256(a, b, c, d, 2, 14); d = op256(d, a, b, c, 7, 8); c = op256(c, d, a, b, 0, 13); b = op256(b, c, d, a, 6, 6);
                    a = op256(a, b, c, d, 13, 5); d = op256(d, a, b, c, 11, 12); c = op256(c, d, a, b, 5, 7); b = op256(b, c, d, a, 12, 5);
                    hh = h2++;
                    _f = G;
                    aa = op256(aa, bb, cc, dd, 15, 9); dd = op256(dd, aa, bb, cc, 5, 7); cc = op256(cc, dd, aa, bb, 1, 15); bb = op256(bb, cc, dd, aa, 3, 11);
                    aa = op256(aa, bb, cc, dd, 7, 8); dd = op256(dd, aa, bb, cc, 14, 6); cc = op256(cc, dd, aa, bb, 6, 6); bb = op256(bb, cc, dd, aa, 9, 14);
                    aa = op256(aa, bb, cc, dd, 11, 12); dd = op256(dd, aa, bb, cc, 8, 13); cc = op256(cc, dd, aa, bb, 12, 5); bb = op256(bb, cc, dd, aa, 2, 14);
                    aa = op256(aa, bb, cc, dd, 10, 13); dd = op256(dd, aa, bb, cc, 0, 13); cc = op256(cc, dd, aa, bb, 4, 7); bb = op256(bb, cc, dd, aa, 13, 5);
                    temp = c; c = cc; cc = temp;

                    _f = IQ;
                    hh = h1++;
                    a = op256(a, b, c, d, 1, 11); d = op256(d, a, b, c, 9, 12); c = op256(c, d, a, b, 11, 14); b = op256(b, c, d, a, 10, 15);
                    a = op256(a, b, c, d, 0, 14); d = op256(d, a, b, c, 8, 15); c = op256(c, d, a, b, 12, 9); b = op256(b, c, d, a, 4, 8);
                    a = op256(a, b, c, d, 13, 9); d = op256(d, a, b, c, 3, 14); c = op256(c, d, a, b, 7, 5); b = op256(b, c, d, a, 15, 6);
                    a = op256(a, b, c, d, 14, 8); d = op256(d, a, b, c, 5, 6); c = op256(c, d, a, b, 6, 5); b = op256(b, c, d, a, 2, 12);
                    hh = h2++;
                    _f = F;
                    aa = op256(aa, bb, cc, dd, 8, 15); dd = op256(dd, aa, bb, cc, 6, 5); cc = op256(cc, dd, aa, bb, 4, 8); bb = op256(bb, cc, dd, aa, 1, 11);
                    aa = op256(aa, bb, cc, dd, 3, 14); dd = op256(dd, aa, bb, cc, 11, 14); cc = op256(cc, dd, aa, bb, 15, 6); bb = op256(bb, cc, dd, aa, 0, 14);
                    aa = op256(aa, bb, cc, dd, 5, 6); dd = op256(dd, aa, bb, cc, 12, 9); cc = op256(cc, dd, aa, bb, 2, 12); bb = op256(bb, cc, dd, aa, 13, 9);
                    aa = op256(aa, bb, cc, dd, 9, 12); dd = op256(dd, aa, bb, cc, 7, 5); cc = op256(cc, dd, aa, bb, 10, 15); bb = op256(bb, cc, dd, aa, 14, 8);
                    temp = d; d = dd; dd = temp;

                    ctx.a += a;
                    ctx.b += b;
                    ctx.c += c;
                    ctx.d += d;
                    ctx.aa += aa;
                    ctx.bb += bb;
                    ctx.cc += cc;
                    ctx.dd += dd;
                    break;
                case 320:
                    HH = HH2;
                    h1 = 0; h2 = 5;
                    aa = ctx.aa; bb = ctx.bb; cc = ctx.cc; dd = ctx.dd; ee = ctx.ee;
                    void op320(ref uint @a, uint @b, ref uint @c, uint @d, uint @e, byte x, byte shift)
                    {
                        @a += _f(@b, @c, @d) + buf[x] + HH[hh];
                        @a = ROL(@a, shift) + @e;
                        @c = ROL(@c, 10);
                    }
                    _f = F;
                    hh = h1++;
                    op320(ref a, b,ref c, d, e, 0, 11); op320(ref e, a,ref b, c, d, 1, 14); op320(ref d, e,ref a, b, c, 2, 15); op320(ref c, d,ref e, a, b, 3, 12);
                    op320(ref b, c,ref d, e, a, 4, 5); op320(ref a, b,ref c, d, e, 5, 8); op320(ref e, a,ref b, c, d, 6, 7); op320(ref d, e,ref a, b, c, 7, 9);
                    op320(ref c, d,ref e, a, b, 8, 11); op320(ref b, c,ref d, e, a, 9, 13); op320(ref a, b,ref c, d, e, 10, 14); op320(ref e, a,ref b, c, d, 11, 15);
                    op320(ref d, e,ref a, b, c, 12, 6); op320(ref c, d,ref e, a, b, 13, 7); op320(ref b, c,ref d, e, a, 14, 9); op320(ref a, b,ref c, d, e, 15, 8);
                    hh = h2++;
                    _f = J;
                    op320(ref aa, bb, ref cc, dd, ee, 5, 8); op320(ref ee, aa, ref bb, cc, dd, 14, 9); op320(ref dd, ee, ref aa, bb, cc, 7, 9); op320(ref cc, dd, ref ee, aa, bb, 0, 11);
                    op320(ref bb, cc, ref dd, ee, aa, 9, 13); op320(ref aa, bb, ref cc, dd, ee, 2, 15); op320(ref ee, aa, ref bb, cc, dd, 11, 15); op320(ref dd, ee, ref aa, bb, cc, 4, 5);
                    op320(ref cc, dd, ref ee, aa, bb, 13, 7); op320(ref bb, cc, ref dd, ee, aa, 6, 7); op320(ref aa, bb, ref cc, dd, ee, 15, 8); op320(ref ee, aa, ref bb, cc, dd, 8, 11);
                    op320(ref dd, ee, ref aa, bb, cc, 1, 14); op320(ref cc, dd, ref ee, aa, bb, 10, 14); op320(ref bb, cc, ref dd, ee, aa, 3, 12); op320(ref aa, bb, ref cc, dd, ee, 12, 6);
                    temp = a; a = aa; aa = temp;
                    _f = G;
                    hh = h1++;
                    op320(ref e, a, ref b, c, d, 7, 7); op320(ref d, e, ref a, b, c, 4, 6); op320(ref c, d, ref e, a, b, 13, 8); op320(ref b, c, ref d, e, a, 1, 13);
                    op320(ref a, b, ref c, d, e, 10, 11); op320(ref e, a, ref b, c, d, 6, 9); op320(ref d, e, ref a, b, c, 15, 7); op320(ref c, d, ref e, a, b, 3, 15);
                    op320(ref b, c, ref d, e, a, 12, 7); op320(ref a, b, ref c, d, e, 0, 12); op320(ref e, a, ref b, c, d, 9, 15); op320(ref d, e, ref a, b, c, 5, 9);
                    op320(ref c, d, ref e, a, b, 2, 11); op320(ref b, c, ref d, e, a, 14, 7); op320(ref a, b, ref c, d, e, 11, 13); op320(ref e, a, ref b, c, d, 8, 12);
                    hh = h2++;
                    _f = IQ;
                    op320(ref ee, aa, ref bb, cc, dd, 6, 9); op320(ref dd, ee, ref aa, bb, cc, 11, 13); op320(ref cc, dd, ref ee, aa, bb, 3, 15); op320(ref bb, cc, ref dd, ee, aa, 7, 7);
                    op320(ref aa, bb, ref cc, dd, ee, 0, 12); op320(ref ee, aa, ref bb, cc, dd, 13, 8); op320(ref dd, ee, ref aa, bb, cc, 5, 9); op320(ref cc, dd, ref ee, aa, bb, 10, 11);
                    op320(ref bb, cc, ref dd, ee, aa, 14, 7); op320(ref aa, bb, ref cc, dd, ee, 15, 7); op320(ref ee, aa, ref bb, cc, dd, 8, 12); op320(ref dd, ee, ref aa, bb, cc, 12, 7);
                    op320(ref cc, dd, ref ee, aa, bb, 4, 6); op320(ref bb, cc, ref dd, ee, aa, 9, 15); op320(ref aa, bb, ref cc, dd, ee, 1, 13); op320(ref ee, aa, ref bb, cc, dd, 2, 11);
                    temp = b; b = bb; bb = temp;
                    _f = H;
                    hh = h1++;
                    op320(ref d, e, ref a, b, c, 3, 11); op320(ref c, d, ref e, a, b, 10, 13); op320(ref b, c, ref d, e, a, 14, 6); op320(ref a, b, ref c, d, e, 4, 7);
                    op320(ref e, a, ref b, c, d, 9, 14); op320(ref d, e, ref a, b, c, 15, 9); op320(ref c, d, ref e, a, b, 8, 13); op320(ref b, c, ref d, e, a, 1, 15);
                    op320(ref a, b, ref c, d, e, 2, 14); op320(ref e, a, ref b, c, d, 7, 8); op320(ref d, e, ref a, b, c, 0, 13); op320(ref c, d, ref e, a, b, 6, 6);
                    op320(ref b, c, ref d, e, a, 13, 5); op320(ref a, b, ref c, d, e, 11, 12); op320(ref e, a, ref b, c, d, 5, 7); op320(ref d, e, ref a, b, c, 12, 5);
                    hh = h2++;
                    _f = H;
                    op320(ref dd, ee, ref aa, bb, cc, 15, 9); op320(ref cc, dd, ref ee, aa, bb, 5, 7); op320(ref bb, cc, ref dd, ee, aa, 1, 15); op320(ref aa, bb, ref cc, dd, ee, 3, 11);
                    op320(ref ee, aa, ref bb, cc, dd, 7, 8); op320(ref dd, ee, ref aa, bb, cc, 14, 6); op320(ref cc, dd, ref ee, aa, bb, 6, 6); op320(ref bb, cc, ref dd, ee, aa, 9, 14);
                    op320(ref aa, bb, ref cc, dd, ee, 11, 12); op320(ref ee, aa, ref bb, cc, dd, 8, 13); op320(ref dd, ee, ref aa, bb, cc, 12, 5); op320(ref cc, dd, ref ee, aa, bb, 2, 14);
                    op320(ref bb, cc, ref dd, ee, aa, 10, 13); op320(ref aa, bb, ref cc, dd, ee, 0, 13); op320(ref ee, aa, ref bb, cc, dd, 4, 7); op320(ref dd, ee, ref aa, bb, cc, 13, 5);
                    temp = c; c = cc; cc = temp;
                    _f = IQ;
                    hh = h1++;
                    op320(ref c, d, ref e, a, b, 1, 11); op320(ref b, c, ref d, e, a, 9, 12); op320(ref a, b, ref c, d, e, 11, 14); op320(ref e, a, ref b, c, d, 10, 15);
                    op320(ref d, e, ref a, b, c, 0, 14); op320(ref c, d, ref e, a, b, 8, 15); op320(ref b, c, ref d, e, a, 12, 9); op320(ref a, b, ref c, d, e, 4, 8);
                    op320(ref e, a, ref b, c, d, 13, 9); op320(ref d, e, ref a, b, c, 3, 14); op320(ref c, d, ref e, a, b, 7, 5); op320(ref b, c, ref d, e, a, 15, 6);
                    op320(ref a, b, ref c, d, e, 14, 8); op320(ref e, a, ref b, c, d, 5, 6); op320(ref d, e, ref a, b, c, 6, 5); op320(ref c, d, ref e, a, b, 2, 12);
                    hh = h2++;
                    _f = G;
                    op320(ref cc, dd, ref ee, aa, bb, 8, 15); op320(ref bb, cc, ref dd, ee, aa, 6, 5); op320(ref aa, bb, ref cc, dd, ee, 4, 8); op320(ref ee, aa, ref bb, cc, dd, 1, 11);
                    op320(ref dd, ee, ref aa, bb, cc, 3, 14); op320(ref cc, dd, ref ee, aa, bb, 11, 14); op320(ref bb, cc, ref dd, ee, aa, 15, 6); op320(ref aa, bb, ref cc, dd, ee, 0, 14);
                    op320(ref ee, aa, ref bb, cc, dd, 5, 6); op320(ref dd, ee, ref aa, bb, cc, 12, 9); op320(ref cc, dd, ref ee, aa, bb, 2, 12); op320(ref bb, cc, ref dd, ee, aa, 13, 9);
                    op320(ref aa, bb, ref cc, dd, ee, 9, 12); op320(ref ee, aa, ref bb, cc, dd, 7, 5); op320(ref dd, ee, ref aa, bb, cc, 10, 15); op320(ref cc, dd, ref ee, aa, bb, 14, 8);
                    temp = d; d = dd; dd = temp;
                    _f = J;
                    hh = h1++;
                    op320(ref b, c, ref d, e, a, 4, 9); op320(ref a, b, ref c, d, e, 0, 15); op320(ref e, a, ref b, c, d, 5, 5); op320(ref d, e, ref a, b, c, 9, 11);
                    op320(ref c, d, ref e, a, b, 7, 6); op320(ref b, c, ref d, e, a, 12, 8); op320(ref a, b, ref c, d, e, 2, 13); op320(ref e, a, ref b, c, d, 10, 12);
                    op320(ref d, e, ref a, b, c, 14, 5); op320(ref c, d, ref e, a, b, 1, 12); op320(ref b, c, ref d, e, a, 3, 13); op320(ref a, b, ref c, d, e, 8, 14);
                    op320(ref e, a, ref b, c, d, 11, 11); op320(ref d, e, ref a, b, c, 6, 8); op320(ref c, d, ref e, a, b, 15, 5); op320(ref b, c, ref d, e, a, 13, 6);
                    hh = h2++;
                    _f = F;
                    op320(ref bb, cc, ref dd, ee, aa, 12, 8); op320(ref aa, bb, ref cc, dd, ee, 15, 5); op320(ref ee, aa, ref bb, cc, dd, 10, 12); op320(ref dd, ee, ref aa, bb, cc, 4, 9);
                    op320(ref cc, dd, ref ee, aa, bb, 1, 12); op320(ref bb, cc, ref dd, ee, aa, 5, 5); op320(ref aa, bb, ref cc, dd, ee, 8, 14); op320(ref ee, aa, ref bb, cc, dd, 7, 6);
                    op320(ref dd, ee, ref aa, bb, cc, 6, 8); op320(ref cc, dd, ref ee, aa, bb, 2, 13); op320(ref bb, cc, ref dd, ee, aa, 13, 6); op320(ref aa, bb, ref cc, dd, ee, 14, 5);
                    op320(ref ee, aa, ref bb, cc, dd, 0, 15); op320(ref dd, ee, ref aa, bb, cc, 3, 13); op320(ref cc, dd, ref ee, aa, bb, 9, 11); op320(ref bb, cc, ref dd, ee, aa, 11, 11);
                    temp = e; e = ee; ee = temp;

                    ctx.a += a;
                    ctx.b += b;
                    ctx.c += c;
                    ctx.d += d;
                    ctx.e += e;
                    ctx.aa += aa;
                    ctx.bb += bb;
                    ctx.cc += cc;
                    ctx.dd += dd;
                    ctx.ee += ee;
                    break;
            }
        }
        internal static RIPEMD_CTX Init(uint size)
        {
            var ctx = new RIPEMD_CTX();
            ctx.a = 0x67452301;
            ctx.b = 0xefcdab89;
            ctx.c = 0x98badcfe;
            ctx.d = 0x10325476;
            ctx.e = 0xc3d2e1f0;
            ctx.aa = 0x76543210;
            ctx.bb = 0xfedcba98;
            ctx.cc = 0x89abcdef;
            ctx.dd = 0x01234567;
            ctx.ee = 0x3c2d1e0f;
            ctx.buffer = uint_buf_reverse.New(16);
            ctx.size = size;
            return ctx;
        }
        internal static void Update(ref RIPEMD_CTX ctx, byte[] data, int length)
        {
            foreach (uint i in ctx.buffer.Push(data, 0, length, 0))
            {
                if (i < length)
                {
                    ProcessBlock(ref ctx);
                }
            }
        }
        internal static byte[] Final(ref RIPEMD_CTX ctx)
        {
            if (ctx.buffer.IsFULL()) ProcessBlock(ref ctx);
            ctx.buffer.PushNext(0x80);
            if (ctx.buffer.IsFULL(2)) ProcessBlock(ref ctx);
            ctx.buffer.Fill(0, 2);
            ctx.buffer.PushTotal();
            ProcessBlock(ref ctx);

            byte[] md = new byte[ctx.size >> 3];
            int i = 0;
            md.CopyFrom(ctx.a, i++ <<2);
            md.CopyFrom(ctx.b, i++ << 2);
            md.CopyFrom(ctx.c, i++ << 2);
            md.CopyFrom(ctx.d, i++ << 2);
            if (ctx.size < 160) return md;

            if (ctx.size < 256)
            {
                md.CopyFrom(ctx.e, i++ << 2);
                return md;
            }

            if (ctx.size == 256)
            {
                md.CopyFrom(ctx.aa, i++ << 2);
                md.CopyFrom(ctx.bb, i++ << 2);
                md.CopyFrom(ctx.cc, i++ << 2);
                md.CopyFrom(ctx.dd, i++ << 2);
                return md;
            }
            if (ctx.size == 320)
            {
                md.CopyFrom(ctx.e, i++ << 2);
                md.CopyFrom(ctx.aa, i++ << 2);
                md.CopyFrom(ctx.bb, i++ << 2);
                md.CopyFrom(ctx.cc, i++ << 2);
                md.CopyFrom(ctx.dd, i++ << 2);
                md.CopyFrom(ctx.ee, i++ << 2);
                return md;
            }
            return md;

        }
    }
    public class RIPEMD160
    {
        public string Make(byte[] data)
        {
            var ctx = RIPEMD.Init(160);
            RIPEMD.Update(ref ctx, data, data.Length);
            return RIPEMD.Final(ref ctx).ToHexString();
        }
    }
    public class RIPEMD128
    {
        public string Make(byte[] data)
        {
            var ctx = RIPEMD.Init(128);
            RIPEMD.Update(ref ctx, data, data.Length);
            return RIPEMD.Final(ref ctx).ToHexString();
        }
    }
    public class RIPEMD256
    {
        public string Make(byte[] data)
        {
            var ctx = RIPEMD.Init(256);
            RIPEMD.Update(ref ctx, data, data.Length);
            return RIPEMD.Final(ref ctx).ToHexString();
        }
    }
    public class RIPEMD320
    {
        public string Make(byte[] data)
        {
            var ctx = RIPEMD.Init(320);
            RIPEMD.Update(ref ctx, data, data.Length);
            return RIPEMD.Final(ref ctx).ToHexString();
        }
    }
}
