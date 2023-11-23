namespace NetPs.Socket.Extras.Security.BlockCipher
{
    using System;
    internal class RSA
    {
        internal static ulong gcd(ulong a, ulong b)
        {
            ulong c;
            while (a != 0)
            {
                c = a;
                a = b % a;
                b = c;
            }
            return b;
        }
        internal static ulong ExtEuclid(ulong a, ulong b)
        {
            ulong x = 0, y = 1, u = 1, v = 0, gcd = b, m, n, q, r;
            while (a != 0)
            {
                q = gcd / a;
                r = gcd % a;
                m = x - u * q;
                n = y - v * q;
                gcd = a;
                a = r;
                x = u;
                y = v;
                u = m;
                v = n;
            }
            return y;
        }
        internal static ulong modmult(ulong a, ulong b, ulong mod)
        {
            if (a == 0) return 0;
            ulong product = a * b;
            if (product / a == b) return product % mod;
            if ((a & 1) == 1)
            {
                product = modmult(a >> 1, b, mod);
                if (product << 1 > product)
                {
                    return ((product << 1) % mod + b) % mod;
                }
            }
            product = modmult(a >> 1, b, mod);
            if (product << 1 > product)
            {
                return (product << 1) % mod;
            }
            ulong sum = 0;
            while (b > 0)
            {
                if ((b & 1) == 1)
                {
                    sum = (sum + a) % mod;
                }
                a = 2 * a % mod;
                b >>= 1;
            }
            return sum;
        }
        internal static long modExp(ulong b, ulong e, ulong m)
        {
            ulong product = 1;
            if (b < 0 || e < 0 || m <= 0) return -1;
            b = b % m;
            while (e > 0)
            {
                if ((e & 1) == 1) product = modmult(product, b, m);
                b = modmult(b, b, m);
                e >>= 1;
            }
            return (long)product;
        }
        internal static void gen_keys()
        {
            ulong prime_count = 0;
            //ulong p = 0, q = 0;
            //ulong e = (2 << 16) + 1;
            //ulong d = 0;
            //ulong max = 0, phi_max = 0;
            var s = RandomSource.New((int)DateTime.Now.Ticks);

            do
            {
                int a = (int)((double)s.Rand() * (prime_count + 1) / (int.MaxValue + 1.0));
                int b = (int)((double)s.Rand() * (prime_count + 1) / (int.MaxValue + 1.0));

            } while (false);
        }
    }
}
