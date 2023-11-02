namespace NetPs.Socket.Extras.Security.MessageDigest
{
    using System;
    public struct MD2_CTX
    {
        internal byte[] buf { get; set; }
        internal byte[] state { get; set; }
        internal byte[] checksum { get; set; }
        internal ulong total { get; set; }
        internal uint used { get; set; }
    }
}
