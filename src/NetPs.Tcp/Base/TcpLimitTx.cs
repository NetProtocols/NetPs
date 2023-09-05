namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Threading;

    public class TcpLimitTx : TcpTx, IDisposable, ISpeedLimit
    {
        private bool is_disposed = false;
        private long last_time { get; set; }
        private int transported_count { get; set; }
        public int Limit { get; protected set; } // must gt 0
        public long LastTime => this.last_time;
        public TcpLimitTx(TcpCore tcpCore) : base(tcpCore)
        {
            this.Limit = -1;
            this.last_time = DateTime.Now.Ticks;
            this.transported_count = 0;
        }

        public override void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            base.Dispose();
        }

        public void SetLimit(int value)
        {
            this.Limit = value;
        }

        protected override void OnBufferTransported()
        {
            if (this.is_disposed) return;
            if (this.Limit > 0)
            {
                this.transported_count += this.nTransported;
                Task.StartNew(wait_limit);
            }
            else
            {
                Task.StartNew(base.restart_transport);
            }
        }

        private void wait_limit()
        {
            if (transported_count > this.Limit)
            {
                var now = DateTime.Now.Ticks;
                if (!this.HasSecondPassed(now))
                {
                    var wait = this.GetWaitMillisecond(now);
                    if (wait > 10)
                    {
                        Thread.Sleep(wait);
                        last_time = now + this.GetMillisecondTicks(wait);
                    }
                    else
                    {
                        last_time = now;
                    }
                }
                else
                {
                    last_time = now;
                }

                this.transported_count = 0;
            }

            this.restart_transport();
        }
    }
}
