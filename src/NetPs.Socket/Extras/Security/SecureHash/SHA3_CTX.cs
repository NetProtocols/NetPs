namespace NetPs.Socket.Extras.Security.SecureHash
{
    using NetPs.Socket.Memory;
    using System;

    public struct SHA3_CTX
    {
        internal uint kind { get; set; }
        internal bool shake { get; set; }
        internal bool keccak { get; set; }
        internal ulong[][] lane { get; set; }
        internal uint b { get; set; }
        internal uint r { get; set; }
        internal uint c { get; set; }
        internal uint nr { get; set; }
        internal uint md_size { get; set; }
        internal bool absorbing { get; set; }
        internal ulong[] buf => buffer.Oo.Data;
        internal ulong_buf_reverse buffer { get; set; }
    }
}
