namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class TcpLimitQueueTx : TcpQueueTx, IDisposable, ISpeedLimit
    {
        private bool is_disposed = false;
        private long last_time { get; set; }
        private int transported_count { get; set; }
        private CancellationToken CancellationToken { get; set; }
        public virtual int Limit { get; protected set; }
        public virtual long LastTime => this.last_time;
        public TcpLimitQueueTx() : base()
        {
            this.CancellationToken = new CancellationToken();
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
            this.CancellationToken.WaitHandle.Close();
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
                Task.StartNew(wait_limit, CancellationToken);
            }
            else
            {
                Task.StartNew(base.OnBufferTransported);
            }
        }

        private async Task wait_limit()
        {
            if (transported_count > this.Limit)
            {
                var now = DateTime.Now.Ticks;
                if (!this.HasSecondPassed(now))
                {
                    var wait = this.GetWaitMillisecond(now);
                    if (wait > 10)
                    {
                        if (CancellationToken.IsCancellationRequested) return;
                        await global::System.Threading.Tasks.Task.Delay(wait, CancellationToken); //阈值10ms, 小于则不等待
                        if (CancellationToken.IsCancellationRequested) return;
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
