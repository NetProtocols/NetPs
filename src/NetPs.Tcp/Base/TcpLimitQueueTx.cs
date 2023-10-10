namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public class TcpLimitQueueTx : TcpQueueTx, IDisposable, ISpeedLimit
    {
        private bool is_disposed = false;
        public virtual int Limit { get; protected set; }
        private int transported_count { get; set; }
        private long last_time { get; set; }
        private ManualResetEvent manualResetEvent { get; set; }
        public TcpLimitQueueTx() : base()
        {
            this.manualResetEvent = new ManualResetEvent(false);
            this.Limit = -1;
            this.transported_count = 0;
            this.last_time = DateTime.Now.Ticks;
        }
        public virtual long LastTime => this.last_time;
        public override void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            this.manualResetEvent.Set();
            this.manualResetEvent.Close();
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
                wait_limit();
            }
            else
            {
                Task.StartNew(base.OnBufferTransported);
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
                        //阈值10ms, 小于则不等待
                        try
                        {
                            if (this.is_disposed || this.manualResetEvent.WaitOne(wait, false))
                            {
                                //终止
                                return;
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            Debug.Assert(false);
                            return;
                        }
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
