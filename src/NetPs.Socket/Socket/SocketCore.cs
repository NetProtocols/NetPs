namespace NetPs.Socket
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
        private bool closed = true;

        protected readonly CompositeDisposable disposables;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketCore"/> class.
        /// </summary>
        public SocketCore()
        {
            this.disposables = new CompositeDisposable();
        }

        /// <summary>
        /// Gets 取消订阅清单.
        /// </summary>
        public virtual CompositeDisposable Disposables => this.disposables;

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
            this.Socket?.Close();
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
            this.Disposables?.Dispose();
            this.Connected = null;
            this.DisConnected = null;
            if (this.Socket != null)
            {
                using (this.Socket)
                {
                    System.Threading.Thread.Sleep(60);
                    this.Socket = null;
                }
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 当链接丢失.
        /// </summary>
        public virtual void OnLoseConnected()
        {
            if (!this.Closed)
            {
                this.closed = true;
                this.DisConnected?.Invoke(this.IPEndPoint);
            }
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
            if (this.Closed)
            {
                this.closed = false;
                this.Connected?.Invoke(this.IPEndPoint);
            }
        }

        /// <summary>
        /// 开始连接.
        /// </summary>
        /// <param name="timeout">超时毫秒数.</param>
        protected virtual async void StartConnect(int timeout)
        {
            var rlt = this.Socket.BeginConnect(this.IPEndPoint, this.ConnectCallback, this.Socket);
            await Task.Delay(timeout);
            if (!rlt.IsCompleted)
            {
                var ok = rlt.AsyncWaitHandle.WaitOne(0, false);
                if (!ok)
                {
                    this.Socket.Close();
                    this.closed = false;
                    this.OnLoseConnected();
                }
            }
        }

        private void ConnectCallback(IAsyncResult asyncResult)
        {
            try
            {
                this.closed = false;
                var client = (Socket)asyncResult.AsyncState;
                client.EndConnect(asyncResult);
                asyncResult.AsyncWaitHandle.Close();
                this.Connecting = false;
                this.closed = true;
                this.OnConnected();
            }
            catch (ObjectDisposedException)
            {
                if (this.Connecting)
                {
                    var ex = new NetPsSocketException(SocketErrorCode.TimedOut, "socket connecting timeout.");
                    this.OnLoseConnected();
                    this.ThrowException(ex);
                }
            }
            catch (SocketException e)
            {
                var ex = new NetPsSocketException(e, this);
                if (!ex.Handled)
                {
                    this.OnLoseConnected();
                    this.ThrowException(e);
                }
            }
            catch (Exception e)
            {
                this.OnLoseConnected();
                this.ThrowException(e);
            }
            finally
            {
                this.Connecting = false;
            }
        }

        //开始监听Accept
        public virtual void Listen(int backlog)
        {
            this.Socket.Listen(backlog);
        }

        //绑定到IPEndPoint
        public virtual void Bind()
        {
            this.Socket.Bind(this.IPEndPoint);
        }
    }
}
