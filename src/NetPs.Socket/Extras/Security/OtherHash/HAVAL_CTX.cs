namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;
    internal class HAVAL_CTX
    {
        internal int passes { get; set; }
        internal int version { get; set; }
        internal int size { get; set; }
        internal uint[] t { get; set; }
        internal uint[] buf => buffer.Oo.Data;
        internal uint_buf_reverse buffer;
        public void SetPasses(int passes) {  this.passes = passes; }
        public void SetVersion(int version) {  this.version = version; }
        public void SetSize(int size) {  this.size = size; }
    }
}
