namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;

    public class UdpTx : IDisposable, IDataTransport
    {
        public UdpCore core { get; set; }
        protected readonly CompositeDisposable disposables;

        private bool is_disposed = false;

        private bool transporting = false;

        private QueueStream cache { get; set; }

        private int state = 1;

        public IPEndPoint RemoteIP { get; }
        private IEndTransport EndTransport { get; set; }
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
            this.cache = SocketCore.StreamPool.GET();
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

        public bool IsDisposed => throw new NotImplementedException();

        public bool Running => throw new NotImplementedException();

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            this.Disposables.Dispose();
            if (this.cache != null)
            {
                SocketCore.StreamPool.PUT(this.cache);
                this.cache = null;
            }
            this.EndTransport = null;
        }

        /// <summary>
        /// 发送数据(添加入发送队列)..
        /// </summary>
        /// <param name="data">数据.</param>
        public virtual void Transport(byte[] data, int offset = 0, int length = -1)
        {
            if (this.is_disposed) return;
            this.TransportCache.Enqueue(data, offset, length);
            this.StartTransport();
        }

        /// <summary>
        /// 开始发送.
        /// </summary>
        public virtual void StartTransport()
        {
            lock (this)
            {
                if (this.transporting || this.is_disposed || (EndTransport == null && !this.core.Receiving)) return;
                this.transporting = true;
            }
            this._Transport();
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
        protected virtual void _Transport()
        {
            if (this.is_disposed) return;
            else if (!this.TransportCache.IsEmpty)
            {
                try
                {
                    if (this.state > 0)
                    {
                        this.buffer = this.TransportCache.Dequeue(this.TransportSize);
                    }

                    this.core.Socket.BeginSendTo(this.buffer, 0, this.buffer.Length, SocketFlags.None, this.RemoteIP, this.SendCallback, null);
                    return;
                }
                //已经释放了
                catch (ObjectDisposedException) { }
                catch (NullReferenceException) { }
                catch (SocketException e)
                {
                    if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Write)) this.core.ThrowException(e);
                }
            }
            end_transport();
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            if (this.is_disposed) return;
            try
            {
                this.state = this.core.Socket.EndSendTo(asyncResult);
                asyncResult.AsyncWaitHandle.Close();
                //传输完成
                this._Transport();
                return;
            }
            //实例释放
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException e)
            {
                if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Write)) this.core.ThrowException(e);
            }


            end_transport();
        }

        public void LookEndTransport(IEndTransport endTransport)
        {
            this.EndTransport = endTransport;
        }
    }
}
