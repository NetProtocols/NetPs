namespace NetPs.Socket.Extras.Security.SecureHash
{
    using System;
    public class SHA1_CTX
    {
        internal ulong total { get; set; }
        internal uint a { get; set; }
        internal uint b { get; set; }
        internal uint c { get; set; }
        internal uint d { get; set; }
        internal uint e { get; set; }
        internal uint used { get; set; }
        internal uint[] buf { get; set; }
    }
}
