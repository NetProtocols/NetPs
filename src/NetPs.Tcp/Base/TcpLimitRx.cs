using NetPs.Socket;
using NetPs.Tcp.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetPs.Tcp
{
    /// <summary>
    /// 限流的Rx
    /// </summary>
    public class TcpLimitRx : TcpRx, IDisposable, ILimit
    {
        private bool is_disposed = false;
        private long last_time { get; set; }
        private int received_count { get; set; }
        public int Limit { get; protected set; } // must gt 0
        public long LastTime => this.last_time;
        public TcpLimitRx(TcpCore tcpCore) : base(tcpCore)
        {
            this.Limit = 0;
            this.received_count = 0;
            this.last_time = DateTime.Now.Ticks;
        }
        void IDisposable.Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
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
            var buffer = new byte[this.nReceived];
            Array.Copy(this.bBuffer, 0, buffer, 0, this.nReceived);
            this.SendReceived(buffer);
            if (this.Limit > 0)
            {
                received_count += this.nReceived;
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
                this.received_count = 0;
            }

            this.restart_receive();
        }
    }
}
