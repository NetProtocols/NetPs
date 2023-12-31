﻿namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Diagnostics;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// tcp 发送控制.
    /// </summary>
    public class TcpTx : BindTcpCore, IDisposable, ITcpTx
    {
        private bool is_disposed = false;
        private bool transporting = false;
        protected int nTransported { get; set; }
        protected TaskFactory Task { get; set; }
        private byte[] buffer { get; set; }
        private int state { get; set; }
        private int offset { get; set; }
        private int length { get; set; }
        private IAsyncResult AsyncResult { get; set; }
        private ITxEvents events { get; set; }
        private IEndTransport EndTransport { get; set; }
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
        public bool IsDisposed => this.is_disposed || this.Core.IsClosed;

        /// <summary>
        /// Gets 发送数据块大小.
        /// </summary>
        public virtual int TransportBufferSize { get; private set; }

        /// <summary>
        /// Gets a value indicating whether 正在发送.
        /// </summary>
        public virtual bool Transporting => this.transporting;
        /// <summary>
        /// 运行状态
        /// </summary>
        public bool Running => this.transporting;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
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
                this.to_end();
                return;
            }
            try
            {
                x_Transport();
            }
            catch { Debug.Assert(false); }
        }

        protected virtual void tell_transported()
        {
            if (to_end())
            {
                if (this.is_disposed) return;
                if (this.events != null) this.events.OnTransported(this);
                if (this.EndTransport != null) this.EndTransport.WhenTransportEnd(this);
                if (this.Transported != null) this.Transported.Invoke(this);
            }
        }

        /// <summary>
        /// 发送队列完成
        /// </summary>
        protected virtual void OnTransported()
        {
            if (this.Core.Actived)
            {
                this.tell_transported();
            }
        }

        /// <summary>
        /// 发送数据.
        /// </summary>
        /// <param name="data">数据.</param>
        private void x_Transport()
        {
            try
            {
                // Socket Poll 判断连接是否可用 this.core.Actived
                if (this.Core.PollWrite(Consts.SocketPollTime))
                {
                    AsyncResult = this.Core.BeginSend(this.buffer, this.offset, this.length, this.SendCallback);
                    if (this.AsyncResult != null)
                    {
                        this.AsyncResult.Wait();
                        return;
                    }
                }
                else
                {
                    if (this.Core.Actived)
                    {
                        restart_transport(); //对方缓冲区已满，重新发送
                        return;
                    }
                }
            }
            catch when (this.Core.IsClosed) { Debug.Assert(false); }
            catch (Exception e) { this.Core.ThrowException(e); }

            to_end();
            if (!this.is_disposed) this.Core.Lose();
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            AsyncResult = null;
            try
            {
                this.state = this.Core.EndSend(asyncResult); //state决定是否冲重传
                if (this.state >= 0)
                {
                    if (this.state == 0)
                    {
                        restart_transport();
                        return;
                    }
                    else
                    {
                        this.OnTransported();
                        return;
                    }
                }
            }
            catch when (this.Core.IsClosed) { Debug.Assert(false); }
            catch (Exception e) { this.Core.ThrowException(e); }

            to_end();
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

        public void SetTransportBufferSize(int size)
        {
            this.TransportBufferSize = size;
        }
    }
}
