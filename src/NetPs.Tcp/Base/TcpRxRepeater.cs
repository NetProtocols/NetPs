using NetPs.Socket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetPs.Tcp
{
    /// <summary>
    /// 转发器
    /// </summary>
    /// <remarks>
    /// limit 不宜设置太大
    /// </remarks>
    public class TcpRxRepeater : TcpRx, IEndTransport, IDisposable
    {
        public const int SECOND = 11000000; // 110% loss time 10% 100ms
        private bool is_disposed = false;
        private bool waiting = false;
        private bool received = false;
        private int realtime_count { get; set; }
        private long last_time { get; set; }
        public IDataTransport Transport { get; protected set; }
        public int Limit { get; protected set; } // must gt 0
        public TcpRxRepeater(TcpCore tcpCore, IDataTransport transport) : base(tcpCore)
        {
            this.Transport = transport;
            this.Transport.LookEndTransport(this);
            this.Limit = -1;
            this.realtime_count = 0;
            this.last_time = DateTime.Now.Ticks;
        }

        public override void Dispose()
        {
            lock (this)
            {
                this.is_disposed = true;
                if (this.Transport != null)
                {
                    this.Transport.Dispose();
                    this.Transport = null;
                }
                base.Dispose();
            }
        }

        /// <summary>
        /// 限制每秒速率
        /// </summary>
        /// <param name="limit">单位 kb/s</param>
        public void SetLimit(int limit)
        {
            this.Limit = limit;
        }
        public override void EndRecevie()
        {
            this.core.Receiving = false;
            if (this.is_disposed || this.nReceived <= 0) return;
            this.limit_transport(this.bBuffer, 0, this.nReceived);
        }

        private void limit_transport(byte[] data, int offset, int length)
        {
            if (length <= 0) return;
            if (this.is_disposed) return;
            this.received = false;
            this.Transport.Transport(data, offset, length);
            this.realtime_count += length - offset;

            this.waiting = true;
            wait_limit();
            lock (this)
            {
                if (this.received || this.Running)
                {
                    this.waiting = false;
                    return;
                }
                this.received = true;
            }
            //this.StartReceive();
            Task.Factory.StartNew(this.StartReceive);
        }
        private void wait_limit()
        {
            if (this.Limit > 0 && this.realtime_count > this.Limit)
            {
                this.realtime_count = 0;
                var now = DateTime.Now.Ticks;
                if (now < SECOND + this.last_time)
                {
                    var wait = 1000 - (int)((now - this.last_time) / 10000);
                    if (wait > 10) Thread.Sleep(wait); //阈值10ms, 小于则不等待
                    this.last_time = DateTime.Now.Ticks;
                }
            }
        }
        public void WhenTransportEnd(IDataTransport transport)
        {
            if (this.is_disposed) return;
            bool run = false;
            lock (this)
            {
                if (!this.Running && !this.waiting && !this.received)  run = true;
                this.received = run;
            }
            if (run)
                this.StartReceive();
        }
    }
}
