namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;
    internal class GOST94_CTX
    {
        internal bool cryptpro { get; set; }
        internal uint[] hash { get; set; }
        internal uint[] sum { get; set; }
        internal uint[] buf => buffer.Oo.Data;
        internal uint_buf_reverse buffer;
        public void SetCryptpro() { cryptpro = true; }
    }

    internal class GOST2012_CTX
    {
        internal uint size { get; set; }
        internal ulong[] hash { get; set; }
        internal ulong[] sum { get; set; }
        internal ulong[] n { get; set; }
        internal ulong[] buf => buffer.Oo.Data;
        internal ulong_buf_reverse buffer;
    }
}
