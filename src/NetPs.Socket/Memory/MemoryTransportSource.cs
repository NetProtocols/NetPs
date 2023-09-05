namespace NetPs.Socket
{
    using System;
    using System.IO;

    /// <summary>
    /// 内存内容
    /// </summary>
    internal class MemoryTransportSource : ITransportSource
    {
        private Stream memory { get; set; }
        private int live_times = 0;
        public MemoryTransportSource(byte[] ms) : this(new MemoryStream(ms)) { }
        public MemoryTransportSource(Stream ms)
        {
            this.memory = ms;
        }
        public int LTimes => live_times;

        public void AddTask(IDataTransport transport)
        {
            this.live_times++;
        }

        public void CopyTo(byte[] buffer, int offset, int count)
        {
            memory.Read(buffer, offset, count);
        }

        public bool IsAlive(IDataTransport transport)
        {
            return false;
        }

        public void RemoveTask(IDataTransport transport)
        {
            this.live_times--;
        }
    }
}
