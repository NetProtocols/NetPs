namespace NetPs.Socket.Extras.Security.GuoMi
{
    using NetPs.Socket.Memory;
    using System;
    public struct SM3_CTX
    {
        internal uint a { get; set; }
        internal uint b { get; set; }
        internal uint c { get; set; }
        internal uint d { get; set; }
        internal uint e { get; set; }
        internal uint f { get; set; }
        internal uint g { get; set; }
        internal uint h { get; set; }

        internal uint[] buf => buffer.Oo.Data;
        internal uint_buf buffer { get; set; }
    }
}
