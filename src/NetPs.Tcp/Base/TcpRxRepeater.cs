namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 转发器
    /// </summary>
    /// <remarks>
    /// limit 不宜设置太大
    /// </remarks>
    public class TcpRxRepeater : TcpRx, IDisposable, IEndTransport, ILimit
    {
        private bool is_disposed = false;
        private bool re_rx = false;
        private bool waiting = false;
        private bool has_limit = false;
        private int transported_count { get; set; }
        private long last_time { get; set; }
        public IDataTransport Transport { get; protected set; }
        public int Limit { get; protected set; }
        public long LastTime => this.last_time;
        private CancellationToken CancellationToken { get; set; }
        public TcpRxRepeater(TcpCore tcpCore, IDataTransport transport) : base(tcpCore)
        {
            this.CancellationToken = new CancellationToken();
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
            this.CancellationToken.WaitHandle.Close();
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
            this.re_rx = false;
            this.has_limit = this.Limit > 0;
            if (this.has_limit)
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
            this.waiting = true;
            this.transported_count += length - offset;
            this.Transport.Transport(data, offset, length);
            Task.StartNew(wait_limit, CancellationToken);
        }
        private async Task wait_limit()
        {
            if (this.transported_count > this.Limit)
            {
                var now = DateTime.Now.Ticks;
                if (!this.HasSecondPassed(now))
                {
                    var wait =this.GetWaitMillisecond(now);
                    if (wait > 10)
                    {
                        await global::System.Threading.Tasks.Task.Delay(wait, CancellationToken); //阈值10ms, 小于则不等待
                        this.last_time = now + this.GetMillisecondTicks(wait);
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
                if (!this.re_rx) return;
            }
            this.restart_receive();
        }
        public void WhenTransportEnd(IDataTransport transport)
        {
            if (this.is_disposed) return;
            lock (this)
            {
                if (this.re_rx) return;
                else if (this.has_limit)
                {
                    this.re_rx = true;
                    if (this.waiting) return;
                }
            }
            restart_receive();
        }
    }
}
