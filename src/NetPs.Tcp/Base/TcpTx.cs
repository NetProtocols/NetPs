namespace NetPs.Tcp
{
    using NetPs.Socket;
    using NetPs.Tcp.Interfaces;
    using System;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// tcp 发送控制.
    /// </summary>
    public class TcpTx : IDisposable, IDataTransport
    {
        public static int i = 0;
        private TcpCore core { get; set; }
        private bool is_disposed = false;
        private bool transporting = false;
        private IEndTransport EndTransport { get; set; }
        private QueueStream cache { get; set; }
        private int state = 1;
        private int real_transport_size;
        public bool IsDisposed => this.is_disposed;
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpTx"/> class.
        /// </summary>
        /// <param name="tcpCore">.</param>
        public TcpTx(TcpCore tcpCore)
        {
            i++;
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
                this.is_disposed = true;
                if (this.cache != null)
                {
                    i--;
                    SocketCore.StreamPool.PUT(this.cache);
                    this.cache = null;
                }
                this.core = null;
                this.EndTransport = null;
            }
        }

        /// <summary>
        /// 发送数据(添加入发送队列)..
        /// </summary>
        /// <param name="data">数据.</param>
        public virtual void Transport(byte[] data, int offset = 0, int length = -1)
        {
            lock (this)
                if (this.is_disposed) end_transport();
                else
                {
                    //fix: NullReferenceException; TransportCache=null
                    if (this.TransportCache != null) this.TransportCache.Enqueue(data, offset, length);
                    else
                    {
                        end_transport();
                        return;
                    }
                    this.StartTransport();
                }
        }

        /// <summary>
        /// 开始发送.
        /// </summary>
        public virtual void StartTransport()
        {
            if (this.transporting || this.is_disposed || this.core == null || (EndTransport == null && !this.core.Receiving)) return;
            this.transporting = true;
            x_Transport();
        }

        private void end_transport()
        {
            lock (this) { this.transporting = false; }
            if (this.EndTransport != null) this.EndTransport.WhenTransportEnd(this);
            if (this.Transported != null) this.Transported.Invoke(this);
        }

        /// <summary>
        /// 发送数据.
        /// </summary>
        /// <param name="data">数据.</param>
        protected virtual void x_Transport()
        {
            if (this.is_disposed || this.core == null) end_transport();
            else if (!this.TransportCache.IsEmpty && this.core.Actived)
            {
                try
                {
                    // Socket Poll 判断连接是否可用 this.core.Actived
                    var poll_ok = this.core.Socket.Poll(Consts.SocketPollTime, SelectMode.SelectWrite);
                    if (poll_ok && this.core != null)
                    {
                        lock (this)
                        {
                            //发送数据为零，使用上次的缓存进行发送
                            if (this.state > 0)
                            {
                                if (this.TransportCache.Length > this.TransportSize) this.real_transport_size = this.TransportSize;
                                else this.real_transport_size = (int)this.TransportCache.Length;
                                this.TransportCache.Dequeue(this.buffer, 0, this.real_transport_size);
                                this.state = 0;
                            }
                        }
                        this.core.Socket.BeginSend(this.buffer, 0, this.real_transport_size, SocketFlags.None, this.SendCallback, null);
                        return;
                    }
                }
                catch (NullReferenceException)
                {
                    //释放
                }
                catch (SocketException e)
                {
                    if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.StartWrite)) this.core.ThrowException(e);
                }
            }
            //发送队列空 or 连接失效
            //socketcore 已经释放，告知传输结束即可
            end_transport();
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                lock (this)
                {
                    if (this.IsDisposed || this.core == null) return;
                    //fix:ObjectDisposedException;Cannot access a disposed object. Object name: 'System.Net.Sockets.Socket'.”
                    this.state = this.core.Socket.EndSend(asyncResult); //state决定是否冲重传
                }
                asyncResult.AsyncWaitHandle.Close();
                //传输完成
                this.x_Transport();
                return;
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException)
            {
                //释放
            }
            catch (SocketException e)
            {
                if (this.core == null || !this.core.Actived) this.core.OnLoseConnected();
                else if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Writing)) this.core.ThrowException(e);
            }
            //释放
            end_transport();
        }

        public void LookEndTransport(IEndTransport endTransport)
        {
            this.EndTransport = endTransport;
        }
    }
}
