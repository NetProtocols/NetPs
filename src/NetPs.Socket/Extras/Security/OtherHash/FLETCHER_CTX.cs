namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    public struct FLETCHER16_CTX
    {
        internal ushort a { get; set; }
        internal ushort b { get; set; }
    }
    public struct FLETCHER32_CTX
    {
        internal uint a { get; set; }
        internal uint b { get; set; }
    }
    public struct FLETCHER64_CTX
    {
        internal ulong a { get; set; }
        internal ulong b { get; set; }
    }
}
