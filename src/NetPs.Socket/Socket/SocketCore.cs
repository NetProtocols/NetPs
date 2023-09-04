namespace NetPs.Socket
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Disposables;

    /// <summary>
    /// 状态改变.
    /// </summary>
    /// <param name="iPEndPoint">ip.</param>
    public delegate void SateChangeHandle(object source);

    /// <summary>
    /// 处理异常.
    /// </summary>
    /// <param name="exception">异常.</param>
    public delegate void SocketExceptionHandler(Exception exception);

    /// <summary>
    /// 套接字基类
    /// </summary>
    public abstract class SocketCore : IDisposable
    {
        /// <summary>
        /// 数据流池
        /// </summary>
        /// <remarks>
        /// 默认池中只保留2个流,可以使用StreamPool.SET_MAX([size])进行扩大。
        /// <br/>
        /// * 需要保证流的带宽不能无限大，这可能导致内存过大。
        /// </remarks>
        public static readonly QueueStreamPool StreamPool = new QueueStreamPool(0x4);
        private ISocketLose socketLose { get; set; }
        private bool is_disposed = false;
        private bool is_closed = true;

        protected readonly CompositeDisposable h_disposables;
        public bool IsDisposed => is_disposed;
        /// <summary>
        /// Initializes a new instance of the <see cref="SocketCore"/> class.
        /// </summary>
        public SocketCore()
        {
            this.h_disposables = new CompositeDisposable();
        }

        public event SateChangeHandle Closed;

        /// <summary>
        /// Gets 取消订阅清单.
        /// </summary>
        public virtual CompositeDisposable Disposables => this.h_disposables;

        /// <summary>
        /// 处理异常.
        /// </summary>
        public virtual event SocketExceptionHandler SocketException;

        /// <summary>
        /// Gets or sets tcp client.
        /// </summary>
        public virtual Socket Socket { get; protected set; }

        /// <summary>
        /// Gets or sets 地址.
        /// </summary>
        public virtual SocketUri Address { get; protected set; }

        /// <summary>
        /// Gets or sets 终端地址.
        /// </summary>
        public virtual IPEndPoint IPEndPoint { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether 存活.
        /// </summary>
        public virtual bool Actived => !this.IsClosed;

        /// <summary>
        /// Gets a value indicating whether gets 链接已关闭.
        /// </summary>
        public virtual bool IsClosed => this.is_closed;

        /// <summary>
        /// Gets 地址.
        /// </summary>
        public EndPoint IP => this.Socket?.RemoteEndPoint;

        /// <summary>
        /// 放置Socket.
        /// </summary>
        /// <param name="socket">Socket.</param>
        public virtual void PutSocket(Socket socket)
        {
            if (this.Socket == null)
            {
                lock (this)
                {
                    if (!this.is_closed) return;
                    this.is_closed = false;
                }
                this.Socket = socket;
            }
        }

        public virtual void IsUdp() => to_opened();

        protected virtual bool to_closed()
        {
            lock (this)
            {
                if (this.is_closed) return false;
                this.is_closed = true;
            }
            return true;
        }

        protected virtual bool to_opened()
        {
            lock (this)
            {
                if (!this.is_closed) return false;
                this.is_closed = false;
            }
            return true;
        }

        /// <summary>
        /// 关闭连接.
        /// </summary>
        public virtual void Close()
        {
            if (!this.is_closed) this.Lose();
        }


        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            this.Disposables.Dispose();
            if (this.Socket != null)
            {
                //防止未初始化socket的情况
                this.Socket.Close();
                //if (this.Socket is IDisposable o) o.Dispose();
                this.Socket = null;
            }
        }

        /// <summary>
        /// 当链接丢失.
        /// </summary>
        public virtual void Lose()
        {
            if (this.is_disposed) return;
            // 交
            if (this.to_closed())
            {
                this.OnClosed();
                this.Closed?.Invoke(this);
                this.tell_lose();
            }
        }

        public virtual void WhenLoseConnected(ISocketLose lose)
        {
            this.socketLose = lose;
        }

        /// <summary>
        /// 抛出异常.
        /// </summary>
        /// <param name="e">异常.</param>
        public virtual void ThrowException(Exception e)
        {
            this.SocketException?.Invoke(e);
        }

        /// <summary>
        /// 当关闭
        /// </summary>
        protected abstract void OnClosed();

        /// <summary>
        /// 当丢失
        /// </summary>
        protected abstract void OnLosed();

        protected virtual void tell_lose()
        {
            this.OnLosed();
            this.socketLose?.OnSocketLosed(this);
        }

        //绑定到IPEndPoint
        public virtual void Bind()
        {
            this.Socket.Bind(this.IPEndPoint);
            this.is_closed = false;
        }
    }
}
