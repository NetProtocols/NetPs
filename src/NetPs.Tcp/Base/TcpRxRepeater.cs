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
    public class TcpRxRepeater : TcpRx, IDisposable, IEndTransport, ILimit
    {
        public const int SECOND = 10000000; // 1 second
        private bool is_disposed = false;
        private bool re_rx = false;
        private bool waiting = false;
        private int transported_count { get; set; }
        private long last_time { get; set; }
        public IDataTransport Transport { get; protected set; }
        public int Limit { get; protected set; } // must gt 0
        public TcpRxRepeater(TcpCore tcpCore, IDataTransport transport) : base(tcpCore)
        {
            this.Transport = transport;
            this.Transport.LookEndTransport(this);
            this.Limit = -1;
            this.transported_count = 0;
            this.last_time = DateTime.Now.Ticks;
        }

        public override void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            if (this.Transport != null)
            {
                this.Transport.Dispose();
                this.Transport = null;
            }
            base.Dispose();
        }

        /// <summary>
        /// 限制每秒速率
        /// </summary>
        /// <param name="limit">单位 kb/s</param>
        public void SetLimit(int limit)
        {
            this.Limit = limit;
        }
        public override void OnRecevied()
        {
            if (this.is_disposed || this.nReceived <= 0) return;
            if (this.Limit > 0)
            {
                this.limit_transport(this.bBuffer, 0, this.nReceived);
            }
            else
            {
                this.Transport.Transport(this.bBuffer, 0, this.nReceived);
            }
        }

        private void limit_transport(byte[] data, int offset, int length)
        {
            this.re_rx = false;
            this.waiting = true;
            this.transported_count += length - offset;
            this.Transport.Transport(data, offset, length);
            Task.Factory.StartNew(wait_limit, CancellationToken);
        }
        private void wait_limit()
        {
            if (this.transported_count > this.Limit)
            {
                var now = DateTime.Now.Ticks;
                if (now < SECOND + this.last_time)
                {
                    var wait =(int)((this.last_time + SECOND - now) / 10000);
                    if (wait > 10)
                    {
                        Thread.Sleep(wait); //阈值10ms, 小于则不等待
                        this.last_time = now + wait;
                    }
                    else
                    {
                        this.last_time = now;
                    }
                }
                else
                {
                    this.last_time = now;
                }
                this.transported_count = 0;
            }

            lock (this)
            {
                this.waiting = false;
                if (this.re_rx) return;
                this.re_rx = true;
            }
            this.restart_receive();
        }
        public void WhenTransportEnd(IDataTransport transport)
        {
            if (this.is_disposed) return;
            lock (this)
            {
                if (this.waiting) return;
                this.re_rx = true;
            }
            Task.Factory.StartNew(this.restart_receive, CancellationToken);
        }
    }
}
