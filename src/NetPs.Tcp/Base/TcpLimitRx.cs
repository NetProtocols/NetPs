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
    public class TcpLimitRx : TcpRx, IDisposable
    {
        public const int SECOND = 70000000; // 70%
        private byte[] x_now { get; set; }
        private bool waiting { get; set; }
        private int realtime_count { get; set; }
        private long last_time { get; set; }
        public IDataTransport Transport { get; protected set; }
        public QueueStream stream { get; set; }
        public int Limit { get; protected set; } // must gt 0
        public TcpLimitRx(TcpCore tcpCore) : base(tcpCore)
        {
            this.stream = SocketCore.StreamPool.GET();
            this.waiting = false;
            this.Limit = 0;
            this.realtime_count = 0;
            this.last_time = DateTime.Now.Ticks;
        }
        void IDisposable.Dispose()
        {
            lock (this)
            {
                if (this.stream != null)
                {
                    SocketCore.StreamPool.PUT(this.stream);
                    this.stream = null;
                }
            }
        }

        /// <summary>
        /// 限制每秒速率
        /// </summary>
        /// <param name="limit">单位 kb/s</param>
        public void SetLimit(int limit)
        {
            this.Limit = limit >> 3;
            x_now = new byte[limit];
        }

        public override void EndRecevie()
        {
            lock (this)
            {
                if (Running && this.nReceived > 0) stream.Enqueue(this.bBuffer, 0, this.nReceived);
                this.core.Receiving = false;
                if (!waiting && this.stream.Length > 0)
                {
                    if (this.Limit > 0 && stream.Length > Limit)
                    {
                        //has cache, cahce is now
                        this.stream.Dequeue(x_now);
                        limit_received(x_now);
                        return;
                    }
                    else
                    {
                        // data is now
                        var now = new byte[this.stream.Length];
                        this.stream.Dequeue(now);
                        limit_received(now);
                        return;
                    }
                }

                if (this.Limit <= 0 || stream.Length < Limit) this.StartReceive();
            }
        }

        private void limit_received(byte[] data)
        {
            this.waiting = true;

            Task.Factory.StartNew(() =>
            {
                this.realtime_count += data.Length;
                if (this.realtime_count > this.Limit)
                {
                    this.realtime_count = 0;
                    var now = DateTime.Now.Ticks;
                    if (now - this.last_time > SECOND) Thread.Sleep(1000 - (int)((now - this.last_time) / 10000000));
                    this.last_time = now;
                }
                this.SendReceived(data);
                this.waiting = false;
            });
        }
    }
}
