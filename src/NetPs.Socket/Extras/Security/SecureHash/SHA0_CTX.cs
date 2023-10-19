namespace NetPs.Socket.Extras.Security.SecureHash
{
    using System;
    public class SHA0_CTX
    {
        internal uint a { get; set; }
        internal uint b { get; set; }
        internal uint c { get; set; }
        internal uint d { get; set; }
        internal uint e { get; set; }
        internal uint[] buf { get; set; }
        internal long total { get; set; }
        internal uint used { get; set; }
    }
}
