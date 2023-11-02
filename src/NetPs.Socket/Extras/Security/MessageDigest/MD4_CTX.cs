namespace NetPs.Socket.Extras.Security.MessageDigest
{
    using NetPs.Socket.Memory;
    using System;
    public struct MD4_CTX
    {
        internal uint a { get; set; }
        internal uint b { get; set; }
        internal uint c { get; set; }
        internal uint d { get; set; }
        internal uint[] buf => buffer.Oo.Data;
        internal uint_buf_reverse buffer;
        public long Total => (long)buffer.Oo.totalbytes;
    }
}
