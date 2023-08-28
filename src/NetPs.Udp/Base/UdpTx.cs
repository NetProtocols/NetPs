namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;

    public class UdpTx : IDisposable
    {
        public readonly UdpCore core;
        protected readonly CompositeDisposable disposables;

        private bool isDisposed = false;

        private bool transporting = false;

        private QueueStream cache;

        private int state = 1;

        public IPEndPoint RemoteIP { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpTx"/> class.
        /// </summary>
        /// <param name="core">.</param>
        public UdpTx(UdpCore udpCore, IPEndPoint endPoint)
        {
            this.disposables = new CompositeDisposable();
            this.core = udpCore;
            this.RemoteIP = endPoint;
            this.TransportSize = Consts.TransportBytes;
            this.cache = new QueueStream();
            this.TransportedObservable = Observable.FromEvent<TransportedHandler, UdpTx>(handler => tx => handler(tx), evt => this.Transported += evt, evt => this.Transported -= evt);
        }

        /// <summary>
        /// Gets 取消订阅清单.
        /// </summary>
        public virtual CompositeDisposable Disposables => this.disposables;

        /// <summary>
        /// Gets or sets 发送完成.
        /// </summary>
        public virtual IObservable<UdpTx> TransportedObservable { get; protected set; }

        /// <summary>
        /// 发送完成.
        /// </summary>
        /// <param name="sender">发送者.</param>
        public delegate void TransportedHandler(UdpTx sender);

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
            this.Transported?.Invoke(this);
            this.Disposables?.Dispose();
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
            else
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

                        //this.core.Socket.BeginSend(this.buffer, 0, this.buffer.Length, SocketFlags.None, this.SendCallback, this.core.Socket);
                        this.core.Socket.BeginSendTo(this.buffer, 0, this.buffer.Length, SocketFlags.None, this.RemoteIP, this.SendCallback, this.core.Socket);
                    }
                    catch (SocketException e)
                    {
                        this.transporting = false;
                        if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Write))
                        {
                            this.core.ThrowException(e);
                        }
                    }
                }
                else
                {
                    this.transporting = false;
                }
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
                    this.state = client.EndSendTo(asyncResult);
                    asyncResult.AsyncWaitHandle.Close();
                }
                catch (SocketException e)
                {
                    this.transporting = false;
                    if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Write))
                    {
                        throw e;
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
