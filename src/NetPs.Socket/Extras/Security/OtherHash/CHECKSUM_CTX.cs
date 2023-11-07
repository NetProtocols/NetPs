namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    public struct CHECKSUM8_CTX
    {
        internal ulong check { get; set; }
    }
    public struct CHECKSUM16_CTX
    {
        internal ulong check { get; set; }
        internal byte tmp { get; set; }
    }
}
