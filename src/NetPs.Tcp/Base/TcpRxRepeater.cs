using NetPs.Socket;
using NetPs.Tcp.Interfaces;
using System;
using System.Drawing;
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
        public const int SECOND = 7000000; // 70%
        private bool waiting { get; set; }
        private int realtime_count { get; set; }
        private long last_time { get; set; }
        public IDataTransport Transport { get; protected set; }
        public QueueStream stream { get; set; }
        public int Limit { get; protected set; } // must gt 0
        public TcpRxRepeater(TcpCore tcpCore, IDataTransport transport) : base(tcpCore)
        {
            this.Transport = transport;
            this.Transport.LookEndTransport(this);
            this.stream = SocketCore.StreamPool.GET();
            this.waiting = false;
            this.Limit = -1;
            this.realtime_count = 0;
            this.last_time = DateTime.Now.Ticks;
        }

        public override void Dispose()
        {
            lock (this)
            {
                if (this.stream != null)
                {
                    SocketCore.StreamPool.PUT(this.stream);
                    this.stream = null;
                }
                if (this.Transport != null)
                {
                    this.Transport.Dispose();
                    this.Transport = null;
                }
            }
            base.Dispose();
        }

        /// <summary>
        /// 限制每秒速率
        /// </summary>
        /// <param name="limit">单位 kb/s</param>
        public void SetLimit(int limit)
        {
            if (limit > 0) limit = limit >> 3;
            this.Limit = limit;
        }
        public static int j = 0;
        public override void EndRecevie()
        {
            j++;
            var send_run = false;
            long max_len = 0;
            var i = 0;
            lock (this)
            {
                //异步回调, 预防空引用, 需要加锁
                if (stream == null) return;
                else if (Running && this.nReceived > 0) stream.Enqueue(this.bBuffer, 0, this.nReceived);
                send_run = !waiting && !this.Transport.Running && this.stream.Length > 0;
                var has_cache = this.Limit > 0 && stream.Length > Limit;
                if (has_cache)
                {
                    //has cache, cahce is now
                    i = this.Limit;
                    max_len = stream.Length - this.Limit;
                }
                else
                {
                    i = (int)this.stream.Length;
                    max_len = 0;
                }
            }

            if (send_run)
            {
                var len = 0;
                this.waiting = true;
                while (true)
                {
                    lock (this)
                    {
                        if (stream == null || this.core == null || this.Transport.IsDisposed) return;
                        if (stream.Length <= max_len) break;
                        if (i > this.ReceiveBufferSize)
                        {
                            i -= this.ReceiveBufferSize;
                            len = this.ReceiveBufferSize;
                        }
                        else
                        {
                            len = i;
                        }
                        this.stream.Dequeue(this.bBuffer, 0, len);
                    }
                    //耗时不宜lock
                    this.limit_transport(this.bBuffer, 0, len);
                }
                bool need_run = false;
                lock (this)
                {
                    this.waiting = false;
                    need_run = this.Transport != null && !this.Transport.Running && this.stream.Length == max_len;
                    if (!need_run) this.core.Receiving = false;
                }
                if (need_run) Task.Factory.StartNew(this.x_StartReceive);
                return;
            }

            if (this.Limit <= 0 || stream.Length < Limit)
            {
                Task.Factory.StartNew(this.x_StartReceive);
                return;
            }
        }

        private void limit_transport(byte[] data, int offset, int length)
        {
            lock (this)
            {
                if (this.Transport == null) return;
                this.Transport.Transport(data, offset, length);
                this.realtime_count += length - offset;
            }

            if (this.Limit > 0 && this.realtime_count > this.Limit)
            {
                this.realtime_count = 0;
                var now = DateTime.Now.Ticks;
                if (now - this.last_time < SECOND)
                {
                    var wait = 700 - (int)((now - this.last_time) / 10000);
                    if (wait > 100) Thread.Sleep(wait);
                }
                this.last_time = now;
            }
        }
        public static int i = 0;
        public void WhenTransportEnd(IDataTransport transport)
        {
            if (this.core == null || transport.IsDisposed) return;
            bool run = false;
            lock (this)
            {
                if (!this.Running && !waiting)
                {
                    this.nReceived = 0;
                    i++;
                    run = true;
                }
            }
            if (run) this.EndRecevie();
        }
    }
}
