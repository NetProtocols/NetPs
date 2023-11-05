namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    public struct ADLER32_CTX
    {
        internal uint a { get; set; }
        internal uint b { get; set; }
    }
    public struct ADLER64_CTX
    {
        internal ulong a { get; set; }
        internal ulong b { get; set; }
    }
}
