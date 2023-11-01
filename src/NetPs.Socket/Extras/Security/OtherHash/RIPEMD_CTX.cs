namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;

    public struct RIPEMD_CTX
    {
        internal uint size { get; set; }
        internal uint a{ get; set; }
        internal uint b { get; set; }
        internal uint c { get; set; }
        internal uint d { get; set; }
        internal uint e { get; set; }

        internal uint aa { get; set; }
        internal uint bb { get; set; }
        internal uint cc { get; set; }
        internal uint dd { get; set; }
        internal uint ee { get; set; }
        internal uint[] buf => buffer.Oo.Data;
        internal uint_buf_reverse buffer;
    }
}
