namespace NetPs.Socket.Extras.Security.OtherHash
{
    using NetPs.Socket.Memory;
    using System;
    public struct TIGER_CTX
    {
        internal ulong a { get; set; }
        internal ulong b { get; set; }
        internal ulong c { get; set; }
        // 额外的计算次数
        internal uint passes { get; set; }
        internal ulong[] buf => buffer.Oo.Data;
        internal ulong_buf_reverse buffer;

        public void SetPasses(int passes)
        {
            if (passes - 3 < 0) return;

            this.passes = (uint)(passes - 3);
        }
    }
}
