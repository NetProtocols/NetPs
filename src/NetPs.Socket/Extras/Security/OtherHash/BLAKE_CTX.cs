namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;
    internal class BLAKE256_CTX
    {
        internal uint size { get; set; }
        internal ulong t { get; set; }
        internal uint[] h { get; set; }
        internal uint[] s { get; set; }
        internal uint[] buf => buffer.Oo.Data;
        internal uint_buf buffer;
    }
    internal class BLAKE512_CTX
    {
        internal uint size { get; set; }
        internal ulong t0 { get; set; }
        internal ulong t1 { get; set; }
        internal ulong[] h { get; set; }
        internal ulong[] s { get; set; }
        internal ulong[] buf => buffer.Oo.Data;
        internal ulong_buf buffer;
    }
    internal class BLAKE2b_CTX
    {
        internal uint size { get; set; }
        internal uint size_key { get; set; }
        internal ulong t0 { get; set; }
        internal ulong t1 { get; set; }
        internal ulong[] h { get; set; }
        internal ulong[] buf => buffer.Oo.Data;
        internal ulong_buf_reverse buffer;
    }
    internal class BLAKE2s_CTX
    {
        internal uint size { get; set; }
        internal uint size_key { get; set; }
        internal ulong t { get; set; }
        internal uint[] h { get; set; }
        internal uint[] buf => buffer.Oo.Data;
        internal uint_buf_reverse buffer;
    }
    internal class BLAKE3_CTX
    {
        internal uint size { get; set; }
        internal uint size_key { get; set; }
        internal ulong t { get; set; }
        internal uint[] h { get; set; }
        internal uint[] buf => buffer.Oo.Data;
        internal uint_buf_reverse buffer;
    }
}
