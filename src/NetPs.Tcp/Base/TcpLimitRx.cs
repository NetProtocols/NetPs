using NetPs.Socket;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NetPs.Tcp
{
    /// <summary>
    /// 限流的Rx
    /// </summary>
    public class TcpLimitRx : TcpRx, IDisposable, ISpeedLimit
    {
        private bool is_disposed = false;
        public virtual int Limit { get; protected set; }
        private int received_count { get; set; }
        private long last_time { get; set; }
        private ManualResetEvent manualResetEvent { get; set; }
        public TcpLimitRx() : base()
        {
            this.manualResetEvent = new ManualResetEvent(false);
            this.Limit = 0;
            this.received_count = 0;
            this.last_time = DateTime.Now.Ticks;
        }
        public virtual long LastTime => this.last_time;
        void IDisposable.Dispose()
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

        /// <summary>
        /// 限制每秒速率
        /// </summary>
        /// <param name="limit">单位 kb/s</param>
        public void SetLimit(int limit)
        {
            this.Limit = limit;
        }

        protected override void OnReceived(byte[] buffer, int length)
        {
            if (this.is_disposed || this.nReceived <= 0) return;
            var buf = new byte[length];
            Array.Copy(buffer, 0, buf, 0, length);
            this.SendReceived(buf);
            if (this.Limit > 0)
            {
                received_count += length;
                wait_limit();
            }
        }

        private void wait_limit()
        {
            if (this.received_count > this.Limit)
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
                this.received_count = 0;
            }

            this.restart_receive();
        }
    }
}
