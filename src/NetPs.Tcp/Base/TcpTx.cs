namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net.Sockets;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;

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
            this.Transported = null;
            if (this.cache != null)
            {
                this.cache.Dispose();
                this.cache = null;
            }
        }

        /// <summary>
        /// 发送数据(添加入发送队列)..
        /// </summary>
        /// <param name="data">数据.</param>
        public virtual void Transport(byte[] data)
        {
            if (this.isDisposed)
            {
                return;
            }

            this.TransportCache.Enqueue(data);
            this.StartTransport();
        }

        /// <summary>
        /// 开始发送.
        /// </summary>
        public virtual void StartTransport()
        {
            if (this.transporting)
            {
                return;
            }

            this.transporting = true;
            this._Transport();
        }

        /// <summary>
        /// 发送数据.
        /// </summary>
        /// <param name="data">数据.</param>
        protected virtual void _Transport()
        {
            if (!this.core.Receiving)
            {
                this.transporting = false;
            }
            // Socket Poll 判断连接是否可用
            else if (this.core.Actived && this.core.Socket.Poll(Consts.SocketPollTime, SelectMode.SelectWrite))
            {
                if (this.isDisposed)
                {
                    return;
                }

                if (!this.TransportCache.IsEmpty)
                {
                    try
                    {
                        if (this.state > 0)
                        {
                            this.buffer = this.TransportCache.Dequeue(this.TransportSize);
                        }

                        this.core.Socket.BeginSend(this.buffer, 0, this.buffer.Length, SocketFlags.None, this.SendCallback, this.core.Socket);
                    }
                    catch (SocketException e)
                    {
                        this.transporting = false;
                        var ex = new NetPsSocketException(e, this.core);
                        if (!ex.Handled)
                        {
                            this.core.ThrowException(ex);
                        }
                    }
                }
                else
                {
                    this.transporting = false;
                    this.Transported?.Invoke(this);
                }
            }
            else
            {
                this.core.OnLoseConnected();
            }
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            if (this.isDisposed)
            {
                return;
            }

            try
            {
                var client = (Socket)asyncResult.AsyncState;
                try
                {
                    this.state = client.EndSend(asyncResult);
                    asyncResult.AsyncWaitHandle.Close();
                }
                catch (SocketException e)
                {
                    this.transporting = false;
                    var ex = new NetPsSocketException(e, this.core);
                    if (!ex.Handled)
                    {
                        throw ex;
                    }
                }

                this._Transport();
            }
            catch (Exception e)
            {
                this.transporting = false;
                this.core.ThrowException(e);
            }
        }
    }
}
