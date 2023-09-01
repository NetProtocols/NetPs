namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// tcp 发送控制.
    /// </summary>
    public class TcpTx : IDisposable, IDataTransport
    {
        private TcpCore core { get; set; }
        private bool is_disposed = false;
        private bool transporting = false;
        private int state { get; set; }
        protected int nTransported { get; set; }
        private QueueStream cache { get; set; }
        private IEndTransport EndTransport { get; set; }
        public bool IsDisposed => this.is_disposed;
        protected TaskFactory Task { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpTx"/> class.
        /// </summary>
        /// <param name="tcpCore">.</param>
        public TcpTx(TcpCore tcpCore)
        {
            this.Task = new TaskFactory(TaskScheduler.Default);
            this.core = tcpCore;
            this.TransportSize = Consts.TransportBytes;
            this.buffer = new byte[this.TransportSize];
            this.cache = SocketCore.StreamPool.GET();
            this.TransportedObservable = Observable.FromEvent<TransportedHandler, TcpTx>(handler => tx => handler(tx), evt => this.Transported += evt, evt => this.Transported -= evt);
        }

        /// <summary>
        /// Gets or sets 发送完成.
        /// </summary>
        public virtual IObservable<TcpTx> TransportedObservable { get; protected set; }

        /// <summary>
        /// 发送完成.
        /// </summary>
        /// <param name="sender">发送者.</param>
        public delegate void TransportedHandler(TcpTx sender);

        /// <summary>
        /// 发送完成.
        /// </summary>
        public virtual event TransportedHandler Transported;

        /// <summary>
        /// Gets 发送队列.
        /// </summary>
        public virtual QueueStream TransportCache => this.cache;

        /// <summary>
        /// Gets 发送数据块大小.
        /// </summary>
        public virtual int TransportSize { get; }

        /// <summary>
        /// Gets a value indicating whether 正在发送.
        /// </summary>
        public virtual bool Transporting => this.transporting;

        private byte[] buffer { get; set; }

        public bool Running => this.transporting;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            if (this.cache != null)
            {
                SocketCore.StreamPool.PUT(this.cache);
                this.cache = null;
            }
            this.buffer = null;
            this.EndTransport = null;
        }

        /// <summary>
        /// 发送数据(添加入发送队列)..
        /// </summary>
        /// <param name="data">数据.</param>
        public virtual void Transport(byte[] data, int offset = 0, int length = -1)
        {
            lock (this) {
                if (this.is_disposed) return;
                this.TransportCache.Enqueue(data, offset, length);
            }

            this.StartTransport();
        }

        /// <summary>
        /// 开始发送.
        /// </summary>
        public virtual void StartTransport()
        {
            lock (this)
            {
                if (this.transporting || (EndTransport == null && !this.core.Receiving)) return;
                this.transporting = true;
                this.state = 1;
            }

            restart_transport();
        }

        protected virtual void restart_transport()
        {
            if (this.is_disposed || !this.core.Actived)
            {
                lock (this)
                {
                    if (!this.transporting) return;
                    this.transporting = false;
                }
                return;
            }

            Task.StartNew(x_Transport);
        }

        protected virtual void tell_transported()
        {
            lock (this)
            {
                if (!this.transporting) return;
                this.transporting = false;
            }
            if (this.EndTransport != null) this.EndTransport.WhenTransportEnd(this);
            if (this.Transported != null) this.Transported.Invoke(this);
        }

        /// <summary>
        /// 发送一次buffer 完成
        /// </summary>
        protected virtual void OnBufferTransported()
        {
            this.restart_transport();
        }

        /// <summary>
        /// 发送队列完成
        /// </summary>
        protected virtual void OnTransported()
        {
            Task.StartNew(this.tell_transported);
        }

        /// <summary>
        /// 发送数据.
        /// </summary>
        /// <param name="data">数据.</param>
        private void x_Transport()
        {
            try
            {
                if (this.is_disposed) return;
                //发送数据为零，使用上次的缓存进行发送
                if (this.state > 0)
                {
                    if (this.cache.Length > this.TransportSize) this.nTransported = this.TransportSize;
                    else this.nTransported = (int)this.cache.Length;
                    this.cache.Dequeue(this.buffer, 0, this.nTransported);
                    this.state = 0;
                }
                if (this.is_disposed || !this.core.Actived) return;
                // Socket Poll 判断连接是否可用 this.core.Actived
                var poll_ok = this.core.Socket.Poll(Consts.SocketPollTime, SelectMode.SelectWrite);
                if (poll_ok)
                {
                    if (this.is_disposed) return;
                    this.core.Socket.BeginSend(this.buffer, 0, this.nTransported, SocketFlags.None, this.SendCallback, null);
                }
                else
                {
                    if (this.is_disposed) return;
                    this.state = 0;
                    restart_transport();
                }
                return;
            }
            catch (ObjectDisposedException) {  }
            catch (NullReferenceException) {  }
            catch (SocketException e)
            {
                if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.StartWrite)) this.core.ThrowException(e);
            }
            if (!this.is_disposed) this.core.Lose();
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                if (this.is_disposed || !this.core.Actived) return;
                this.state = this.core.Socket.EndSend(asyncResult); //state决定是否冲重传
                asyncResult.AsyncWaitHandle.Close();
                if (this.is_disposed) return;
                //传输完成
                if (this.cache.IsEmpty)
                {
                    this.OnTransported();
                }
                else
                {
                    this.OnBufferTransported();
                }
                return;
            }
            catch (ObjectDisposedException) {  }
            catch (NullReferenceException) {  }
            catch (SocketException e)
            {
                if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Writing)) this.core.ThrowException(e);
                return;
            }
            if (!this.is_disposed) this.core.Lose();
        }

        public void LookEndTransport(IEndTransport endTransport)
        {
            this.EndTransport = endTransport;
        }
    }
}
