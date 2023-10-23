namespace NetPs.Socket.Extras.Security.MessageDigest
{
    using NetPs.Socket.Memory;
    using System;

    public struct MD6_CTX
    {
        internal LoopArray<uint> r_buf;
        internal uint r { get; set; }
        internal long total { get; set; }
        internal int used { get; set; }
        internal byte[] buf { get; set; }
        internal uint[] key { get; set; }
        internal uint key_len { get; set; }
        internal uint size { get; set; }
        internal uint levels { get; set; }
        internal uint[][] hash_buf { get; set; }
        internal byte[] hash_ix { get; set; }
        internal uint[] hash_times { get; set; }
        internal uint times { get; set; }
        public int Levels => (int)this.levels;
        public int KeyLength => (int)this.key_len;
        internal static void PrepareKey(ref MD6_CTX c, byte[] key, int len)
        {
            if (len < 0) return;
            if (len > MD6.MD6_KEY_SIZE) len = MD6.MD6_KEY_SIZE;
            uint i, j;
            for (i = 0, j = 0; (i < 16) && ((j + 3) < len); i++, j += 4)
            {
                c.key[i] = (uint)((key[j] << 24) | (key[j + 1] << 16) | (key[j + 2] << 8) | key[j + 3]);
            }
            if (j < len)
            {
                c.key[i] = (uint) key[j] << 24;
                if ((len & 0b11) != 0)
                {
                    c.key[i] |= (uint)((key[j + 1] << 16));
                    if ((len & 0b1) != 0)
                    {
                        c.key[i] |= (uint)(key[j + 2] << 8);
                    }
                }
                i++;
            }
            c.key_len = i;
        }
        public void SetKEY(byte[] key, int length)
        {
            if (key == null) return;
            PrepareKey(ref this, key, length);
        }
        public void SetLevels(int levels)
        {
            if (levels != 128 && levels != 256 && levels != 512) levels = 128;
            this.levels = (uint)levels;
        }
        public void SetSize(int size)
        {
            this.size = (uint)size;
            this.r = (40 + this.size / 4);
            if (this.r < MD6.MD6_RAW_SIZE)
            {
                this.r_buf = LoopArray<uint>.New(MD6.MD6_RAW_SIZE);
            }
            else
            {
                this.r_buf = LoopArray<uint>.New((int)this.r);
            }
        }
    }
}
