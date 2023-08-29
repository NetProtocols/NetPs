﻿namespace NetPs.Socket
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Disposables;
    using System.Threading.Tasks;

    /// <summary>
    /// 状态改变.
    /// </summary>
    /// <param name="iPEndPoint">ip.</param>
    public delegate void SateChangeHandler(IPEndPoint iPEndPoint);

    /// <summary>
    /// 处理异常.
    /// </summary>
    /// <param name="exception">异常.</param>
    public delegate void SocketExceptionHandler(Exception exception);

    /// <summary>
    /// .
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
        private bool disposed = false;
        private bool closed = true;

        protected readonly CompositeDisposable h_disposables;
        public bool IsDisposed => disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketCore"/> class.
        /// </summary>
        public SocketCore()
        {
            this.h_disposables = new CompositeDisposable();
        }

        /// <summary>
        /// Gets 取消订阅清单.
        /// </summary>
        public virtual CompositeDisposable Disposables => this.h_disposables;

        /// <summary>
        /// 连接.
        /// </summary>
        public virtual event SateChangeHandler Connected;

        /// <summary>
        /// 丢失连接.
        /// </summary>
        public virtual event SateChangeHandler DisConnected;

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
        /// Gets or sets 连接超时毫秒(ms).
        /// </summary>
        public virtual int ConnectTimeout { get; set; } = 3600;

        /// <summary>
        /// Gets a value indicating whether 存活.
        /// </summary>
        public virtual bool Actived => !this.Closed && (this.Socket?.Connected ?? false);

        /// <summary>
        /// Gets a value indicating whether gets 链接已关闭.
        /// </summary>
        public virtual bool Closed => this.closed;

        /// <summary>
        /// Gets 地址.
        /// </summary>
        public EndPoint IP => this.Socket?.RemoteEndPoint;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets 正在连接.
        /// </summary>
        public virtual bool Connecting { get; protected set; }

        /// <summary>
        /// 放置Socket.
        /// </summary>
        /// <param name="socket">Socket.</param>
        public void PutSocket(Socket socket)
        {
            if (this.Socket == null)
            {
                this.closed = false;
                this.Socket = socket;
            }
        }

        /// <summary>
        /// 关闭连接.
        /// </summary>
        public virtual void Close()
        {
            if (this.Socket != null && !this.Closed) this.Socket.Close();
            this.OnLoseConnected();
        }

        public virtual void Shutdown()
        {
            if (this.Actived)
            {
                //主动关闭的必要：发送 FIN 包
                this.Socket.Shutdown(SocketShutdown.Both);
            }
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            this.disposed = true;
            if (this.Socket != null)
            {
                Close();
                if (this.Actived) Shutdown();
                if (this.Socket is IDisposable o) o.Dispose();
                this.Socket = null;
            }
            this.Disposables.Dispose();
            //GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 当链接丢失.
        /// </summary>
        public virtual void OnLoseConnected()
        {
            if (!this.Closed)
            {
                this.closed = true;
                this.socketLose?.SocketLosed(this);
                DisConnected?.Invoke(this.IPEndPoint);
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
        /// 当链接开始.
        /// </summary>
        protected virtual void OnConnected()
        {
            if (!this.Closed) this.Connected?.Invoke(this.IPEndPoint);
        }

        /// <summary>
        /// 开始连接.
        /// </summary>
        /// <param name="timeout">超时毫秒数.</param>
        protected virtual async void StartConnect(int timeout)
        {
            this.closed = true;

            var rlt = this.Socket.BeginConnect(this.IPEndPoint, this.ConnectCallback, this.Socket);
            await Task.Delay(timeout);
            if (!rlt.IsCompleted)
            {
                var ok = rlt.AsyncWaitHandle.WaitOne(0, false);
                if (!ok)
                {
                    this.Socket.Close();
                    this.closed = true;
                    this.OnLoseConnected();
                }
            }
        }
        private void ConnectCallback(IAsyncResult asyncResult)
        {
            var client = (Socket)asyncResult.AsyncState;
            try
            {
                client.EndConnect(asyncResult);
                asyncResult.AsyncWaitHandle.Close();
                this.Connecting = false;
                this.closed = false;
                this.OnConnected();
            }
            catch (ObjectDisposedException)
            {
                this.Connecting = false;
                if (this.Connecting)
                {
                    this.OnLoseConnected();
                }
            }
            catch (SocketException e)
            {
                this.Connecting = false;
                if (!NetPsSocketException.Deal(e, this, NetPsSocketExceptionSource.Connect))
                {
                    this.OnLoseConnected();
                    this.ThrowException(e);
                }
            }
        }

        //开始监听Accept
        public virtual void Listen(int backlog)
        {
            this.Socket.Listen(backlog);
            this.closed = false;
        }

        //绑定到IPEndPoint
        public virtual void Bind()
        {
            this.Socket.Bind(this.IPEndPoint);
            this.closed = false;
        }
    }
}
