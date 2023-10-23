namespace NetPs.Socket.Extras.Security.SecureHash
{
    using NetPs.Socket.Memory;
    using System;
    public struct SHA0_CTX
    {
        internal uint a { get; set; }
        internal uint b { get; set; }
        internal uint c { get; set; }
        internal uint d { get; set; }
        internal uint e { get; set; }
        internal uint[] buf => buffer.Oo.Data;
        public long Total => (long)buffer.Oo.totalbytes;
        internal uint_buf buffer { get; set; }
    }
}
