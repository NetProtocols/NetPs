namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public class UdpRxRepeater : UdpRx, IDisposable, IEndTransport, ISpeedLimit
    {
        private bool is_disposed = false;
        private bool re_rx = false;
        private bool waiting = false;
        private bool has_limit = false;
        private long last_time { get; set; }
        private int transported_count { get; set; }
        public IDataTransport Transport { get; protected set; }
        private ManualResetEvent manualResetEvent { get; set; }
        public UdpRxRepeater()
        {
            this.manualResetEvent = new ManualResetEvent(false);
            this.Limit = -1;
            this.transported_count = 0;
            this.last_time = DateTime.Now.Ticks;
        }
        public virtual int Limit { get; protected set; }
        public virtual long LastTime => this.last_time;
        public virtual void SetLimit(int limit)
        {
            this.Limit = limit;
        }
        public virtual void BindTransport(IDataTransport transport)
        {
            this.Transport = transport;
            this.Transport.LookEndTransport(this);
        }
        public override void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            if (this.Transport != null && !this.Transport.IsDisposed)
            {
                this.Transport.Dispose();
            }
            this.manualResetEvent.Set();
            this.manualResetEvent.Close();
            base.Dispose();
        }

        protected override void OnReceived()
        {
            if (this.is_disposed || this.nReceived <= 0) return;
            if (this.Transport.IsDisposed) return;
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
            if (this.Transport == null) return;
            this.Transport.Transport(data, offset, length);

            wait_limit();
        }
        private void wait_limit()
        {
            if (this.transported_count > this.Limit)
            {
                var now = DateTime.Now.Ticks;
                if (!this.HasSecondPassed(now))
                {
                    var wait = this.GetWaitMillisecond(now);
                    //阈值10ms, 小于则不等待
                    if (wait > 10)
                    {
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

            if (this.Transport is IUdpTx tx)
            {
                if (!tx.Core.Actived) return;
            }
            if (this.Transport.IsDisposed) return;

            this.restart_receive();
        }
        public void WhenTransportEnd(IDataTransport transport)
        {
            if (this.is_disposed) return;
            if (this.Transport.IsDisposed) return;

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
