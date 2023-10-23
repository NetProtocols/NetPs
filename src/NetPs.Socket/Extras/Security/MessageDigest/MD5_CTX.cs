namespace NetPs.Socket.Extras.Security.MessageDigest
{
    using System;
    public struct MD5_CTX
    {
        internal uint a { get; set; }
        internal uint b { get; set; }
        internal uint c { get; set; }
        internal uint d { get; set; }
        internal int used { get; set; }
        internal byte[] buf { get; set; }
        internal long total { get; set; }
        public long Total => total;
    }
}
