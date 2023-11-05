namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;
    public struct MURMUR_X86_128_CTX
    {
        internal uint seed { get; set; }
        internal uint h1 { get; set; }
        internal uint h2 { get; set; }
        internal uint h3 { get; set; }
        internal uint h4 { get; set; }
        internal uint[] buf => buffer.Oo.Data;
        internal uint_buf_reverse buffer;
        public void SetSeed(int seed)
        {
            this.seed = (uint)seed;
            this.h1 = this.seed;
            this.h2 = this.seed;
            this.h3 = this.seed;
            this.h4 = this.seed;
        }
    }
    public struct MURMUR_X86_32_CTX
    {
        internal uint seed { get; set; }
        internal uint h1 { get; set; }
        internal uint[] buf => buffer.Oo.Data;
        internal uint_buf_reverse buffer;
        public void SetSeed(int seed)
        {
            this.seed = (uint)seed;
            this.h1 = this.seed;
        }
    }
    public struct MURMUR_X64_128_CTX
    {
        internal ulong seed { get; set; }
        internal ulong h1 { get; set; }
        internal ulong h2 { get; set; }
        internal ulong[] buf => buffer.Oo.Data;
        internal ulong_buf_reverse buffer;
        public void SetSeed(long seed)
        {
            this.seed = (ulong)seed;
            this.h1 = this.seed;
            this.h2 = this.seed;
        }
    }
}
