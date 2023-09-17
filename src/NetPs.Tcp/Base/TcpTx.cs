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
    public class TcpTx : IDisposable, ITx, IBindTcpCore
    {
        private bool is_disposed = false;
        private bool transporting = false;
        private int state { get; set; }
        protected int nTransported { get; set; }
        private IEndTransport EndTransport { get; set; }
        protected TaskFactory Task { get; set; }
        private IAsyncResult AsyncResult { get; set; }
        private ITxEvents events { get; set; }
        private byte[] buffer { get; set; }
        private int offset { get; set; }
        private int length { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpTx"/> class.
        /// </summary>
        /// <param name="tcpCore">.</param>
        public TcpTx()
        {
            this.Task = new TaskFactory(TaskScheduler.Default);
            this.TransportBufferSize = Consts.TransportBytes;
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
        /// 是否释放
        /// </summary>
        public bool IsDisposed => this.is_disposed;

        /// <summary>
        /// Gets 发送数据块大小.
        /// </summary>
        public virtual int TransportBufferSize { get; }

        /// <summary>
        /// Gets a value indicating whether 正在发送.
        /// </summary>
        public virtual bool Transporting => this.transporting;
        /// <summary>
        /// 运行状态
        /// </summary>
        public bool Running => this.transporting;

        public TcpCore Core { get; private set; }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            if (this.AsyncResult != null)
            {
                SocketCore.WaitHandle(AsyncResult, () => { });
                //{
                //    if (this.Core.CanEnd)
                //    {
                //        this.Core.Socket.EndSend(AsyncResult);
                //    }
                //});
                this.AsyncResult = null;
            }
            this.EndTransport = null;
            this.events?.OnDisposed(this);
        }

        /// <summary>
        /// 发送数据(添加入发送队列)..
        /// </summary>
        /// <param name="data">数据.</param>
        public virtual void Transport(byte[] data, int offset, int length)
        {
            if (this.is_disposed || length == 0) return;
            if (length < 0 || length > data.Length) length = data.Length;
            if (length > this.TransportBufferSize) throw new ArgumentException("tcp tx buffer length overflow.");

            if (this.to_start())
            {
                this.events?.OnTransporting(this);
                this.StartTransport(data, offset, length);
            }
        }

        /// <summary>
        /// 开始发送.
        /// </summary>
        protected virtual void StartTransport(byte[] data, int offset, int length)
        {
            this.buffer = data;
            this.offset = offset;
            this.length = length;

            restart_transport();
        }

        protected virtual bool to_start()
        {
            lock (this)
            {
                if (this.transporting || (EndTransport == null && !this.Core.Receiving)) return false;
                this.transporting = true;
            }
            return true;
        }
        protected virtual bool to_end()
        {
            lock (this)
            {
                if (!this.transporting) return false;
                this.transporting = false;
            }
            return true;
        }

        protected virtual void restart_transport()
        {
            if (this.is_disposed || !this.Core.Actived)
            {
                to_end();
                return;
            }

            Task.StartNew(x_Transport);
        }

        protected virtual void tell_transported()
        {
            if (to_end())
            {
                this.events?.OnTransported(this);
                if (this.EndTransport != null) this.EndTransport.WhenTransportEnd(this);
                if (this.Transported != null) this.Transported.Invoke(this);
            }
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
                if (this.is_disposed || !this.Core.Actived) return;
                // Socket Poll 判断连接是否可用 this.core.Actived
                var poll_ok = this.Core.Socket.Poll(Consts.SocketPollTime, SelectMode.SelectWrite);
                if (poll_ok)
                {
                    if (this.Core.CanBegin)
                    {
                        if (this.is_disposed) return;
                        AsyncResult = this.Core.Socket.BeginSend(this.buffer, this.offset, this.length, SocketFlags.None, this.SendCallback, null);
                    }
                }
                else
                {
                    if (this.is_disposed) return;
                    restart_transport(); //对方缓冲区已满，重新发送
                }
                return;
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException e)
            {
                AsyncResult = null;
                if (!NetPsSocketException.Deal(e, this.Core, NetPsSocketExceptionSource.StartWrite)) this.Core.ThrowException(e);
            }
            if (!this.is_disposed) this.Core.Lose();
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            AsyncResult = null;
            try
            {
                if (this.is_disposed || !this.Core.Actived) return;
                if (this.Core.CanEnd)
                {
                    this.state = this.Core.Socket.EndSend(asyncResult); //state决定是否冲重传
                    if (this.state <= 0)
                    {
                        restart_transport();
                    }
                }
                else
                {
                    asyncResult.AsyncWaitHandle.Close();
                    if (!this.is_disposed) this.Core.Lose();
                    return;
                }
                asyncResult.AsyncWaitHandle.Close();
                if (this.is_disposed) return;
                this.OnTransported();
                return;
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException e)
            {
                if (!NetPsSocketException.Deal(e, this.Core, NetPsSocketExceptionSource.Writing)) this.Core.ThrowException(e);
                return;
            }
            if (!this.is_disposed) this.Core.Lose();
        }

        public void LookEndTransport(IEndTransport endTransport)
        {
            this.EndTransport = endTransport;
        }

        public void BindEvents(ITxEvents events)
        {
            this.events = events;
        }

        public void BindCore(TcpCore core)
        {
            this.Core = core;
        }
    }
}
