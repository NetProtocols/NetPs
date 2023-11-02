namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;
    public struct WHIRLPOOL_CTX
    {
        internal ulong[] state { get; set; }
        internal ulong[] buf => buffer.Oo.Data;
        internal ulong_buf buffer;
    }
}
