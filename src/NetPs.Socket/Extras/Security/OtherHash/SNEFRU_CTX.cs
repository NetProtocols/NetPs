namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;
    public struct SNEFRU_CTX
    {
        internal uint size { get; set; }
        internal int hash_size { get; set; }
        internal uint[] hash { get; set; }
        internal uint[] buf => buffer.Oo.Data;
        internal uint_buf buffer;
    }
}
