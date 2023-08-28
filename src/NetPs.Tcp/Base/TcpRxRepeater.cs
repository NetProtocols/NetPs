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
        public const int SECOND = 70000000; // 70%
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
            if (this.stream != null)
            {
                SocketCore.StreamPool.PUT(this.stream);
                this.stream = null;
            }
            if (!this.Transport.IsDisposed) this.Transport.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// 限制每秒速率
        /// </summary>
        /// <param name="limit">单位 kb/s</param>
        public void SetLimit(int limit)
        {
            this.Limit = limit >> 3;
        }

        public override void EndRecevie()
        {
            if (Running && this.nReceived > 0) stream.Enqueue(this.bBuffer, 0, this.nReceived);

            this.core.Receiving = false;
            if (!waiting && !this.Transport.Running && this.stream.Length > 0)
            {
                if (this.Limit > 0 && stream.Length > Limit)
                {
                    var len = stream.Length - this.Limit;
                    var i = this.Limit;
                    this.waiting = true;
                    while (stream.Length > len)
                    {
                        if (this.core == null || this.Transport.IsDisposed) return;
                        if (i > this.ReceiveBufferSize)
                        {
                            i -= this.ReceiveBufferSize;
                            this.stream.Dequeue(this.bBuffer, 0, this.ReceiveBufferSize);
                            this.limit_transport(this.bBuffer, 0, this.ReceiveBufferSize);
                        }
                        else
                        {
                            this.stream.Dequeue(this.bBuffer, 0, i);
                            this.limit_transport(this.bBuffer, 0, i);
                        }
                    }
                    this.waiting = false;
                    if (!this.Transport.Running) this.WhenTransportEnd(this.Transport);
                    //has cache, cahce is now
                    return;
                }
                else
                {
                    // data is now
                    this.waiting = true;
                    while (stream.Length > 0)
                    {
                        if (this.core == null || this.Transport.IsDisposed) return;
                        if (stream.Length > this.ReceiveBufferSize)
                        {
                            this.stream.Dequeue(this.bBuffer, 0, this.ReceiveBufferSize);
                            this.limit_transport(this.bBuffer, 0, this.ReceiveBufferSize);
                        }
                        else
                        {
                            var len = this.stream.Dequeue(this.bBuffer, 0, (int)stream.Length);
                            this.limit_transport(this.bBuffer, 0, len);
                        }
                    }
                    this.waiting = false;
                    if (!this.Transport.Running) this.WhenTransportEnd(this.Transport);
                    return;
                }
            }

            if (this.Limit <= 0 || stream.Length < Limit)
            {

                this.StartReceive();
                return;
            }
        }

        private void limit_transport(byte[] data, int offset, int length)
        {
            lock (this)
            {
                this.Transport.Transport(data, offset, length);
                this.realtime_count += length - offset;
            }

            if (this.Limit > 0 && this.realtime_count > this.Limit)
            {
                this.realtime_count = 0;
                var now = DateTime.Now.Ticks;
                if (now - this.last_time < SECOND)
                {
                    var wait = 700 - (int)((now - this.last_time) / 10000000);
                    if (wait > 100) Thread.Sleep(wait);
                }
                this.last_time = now;
            }
        }

        public void WhenTransportEnd(IDataTransport transport)
        {
            if (transport.IsDisposed || this.core == null) return;
            lock (this)
            {
                if (!this.Running && !waiting)
                {
                    this.nReceived = 0;
                    this.EndRecevie();
                }
            }
        }
    }
}
