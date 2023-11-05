namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    public struct FNV32_CTX
    {
        internal uint hval { get; set; }
        internal bool mode_a { get; set; }
        public void A_MODE() { mode_a = true; }
    }
    public struct FNV64_CTX
    {
        internal ulong hval { get; set; }
        internal bool mode_a { get; set; }
        public void A_MODE() { mode_a = true; }
    }
}
