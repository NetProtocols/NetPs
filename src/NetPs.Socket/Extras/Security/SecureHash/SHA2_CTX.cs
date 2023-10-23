namespace NetPs.Socket.Extras.Security.SecureHash
{
    using NetPs.Socket.Memory;
    using System;
    public struct SHA256_CTX
    {
        internal uint a { get; set; }
        internal uint b { get; set; }
        internal uint c { get; set; }
        internal uint d { get; set; }
        internal uint e { get; set; }
        internal uint f { get; set; }
        internal uint g { get; set; }
        internal uint h { get; set; }
        internal uint[] buf => buffer.Data;
        internal uint_buf buffer { get; set; }
    }
    public struct SHA512_CTX
    {
        internal ulong a { get; set; }
        internal ulong b { get; set; }
        internal ulong c { get; set; }
        internal ulong d { get; set; }
        internal ulong e { get; set; }
        internal ulong f { get; set; }
        internal ulong g { get; set; }
        internal ulong h { get; set; }
        internal ulong[] buf => buffer.Data;
        internal ulong_buf buffer { get; set; }
    }
}
