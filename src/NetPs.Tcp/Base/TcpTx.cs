namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net.Sockets;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Threading;

    /// <summary>
    /// tcp 发送控制.
    /// </summary>
    public class TcpTx : IDisposable
    {
        private readonly TcpCore core;

        private bool isDisposed = false;

        private bool transporting = false;

        private QueueStream cache;

        private int state = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpTx"/> class.
        /// </summary>
        /// <param name="tcpCore">.</param>
        public TcpTx(TcpCore tcpCore)
        {
            this.core = tcpCore;
            this.TransportSize = Consts.TransportBytes;
            this.cache = new QueueStream();
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

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            this.isDisposed = true;
            if (this.cache != null)
            {
                lock (this.cache) this.cache.Dispose();
                this.cache = null;
            }
        }

        /// <summary>
        /// 发送数据(添加入发送队列)..
        /// </summary>
        /// <param name="data">数据.</param>
        public virtual void Transport(byte[] data)
        {
            if (this.isDisposed) end_transport();
            else
            {
                //fix: NullReferenceException; TransportCache=null
                if (this.TransportCache != null) this.TransportCache.Enqueue(data);
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
            if (this.transporting) return;
            this.transporting = true;
            this.x_Transport();
        }

        private void end_transport()
        {
            this.transporting = false;
            if (this.Transported != null) this.Transported.Invoke(this);
        }

        /// <summary>
        /// 发送数据.
        /// </summary>
        /// <param name="data">数据.</param>
        protected virtual void x_Transport(bool need_receive = true)
        {
            if (need_receive && !this.core.Receiving)
            {
                //必须存在接收
                this.transporting = false;
            }

            else if (this.isDisposed || this.core.IsDisposed) end_transport();
            // Socket Poll 判断连接是否可用 this.core.Actived
            else if (!this.TransportCache.IsEmpty && this.core.Actived)
        {
                //发送数据为零，使用上次的缓存进行发送
                if (this.state > 0)
                {
                    this.buffer = this.TransportCache.Dequeue(this.TransportSize);
                }
                this.state = 0;
                try
                {
                    var poll_ok = this.core.Socket.Poll(Consts.SocketPollTime, SelectMode.SelectWrite);
                    if (poll_ok)
                        //fix: 0x0000005 Access violation 空引用
                        if (this.core.Socket != null && !this.core.Closed) this.core.Socket.BeginSend(this.buffer, 0, this.buffer.Length, SocketFlags.None, this.SendCallback, this.core.Socket);
                        else
                        {
                            this.core.OnLoseConnected();
                            end_transport();
                        }
                    else end_transport();
                }
                catch (Exception e)
                {
                    if (this.core.Closed)
                    {
                        //socketcore 已经释放，告知传输结束即可
                    }
                    else if (e is SocketException socket_e)
                    {
                        var ex = new NetPsSocketException(socket_e, this.core, NetPsSocketExceptionSource.StartWrite);
                        if (!ex.Handled) this.core.ThrowException(ex);
                    }
                    //不是在传输过程中报错，不需要重传
                    end_transport();
                }
            }
            else
            {
                //发送队列空 or 连接失效
                end_transport();
            }
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            var client = (Socket)asyncResult.AsyncState;
            try
            {
                //fix:ObjectDisposedException;Cannot access a disposed object. Object name: 'System.Net.Sockets.Socket'.”
                if (!this.core.Closed) this.state = client.EndSend(asyncResult); //state决定是否冲重传
            }
            catch (Exception e)
            {
                if (this.core.Closed)
                {
                    //Socket ObjectDisposedException 连接已经关闭，退出即可不用任何操作
                    end_transport();
                    return;
                }
                else if (e is SocketException socket_e)
                {
                    var ex = new NetPsSocketException(socket_e, this.core, NetPsSocketExceptionSource.Writing);
                    if (!ex.Handled) this.core.ThrowException(ex);
                }
            }
            //传输完成
            this.x_Transport();
            asyncResult.AsyncWaitHandle.Close();

        }
    }
}
