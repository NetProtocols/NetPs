namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    public class UdpTx : BindUdpCore, IDisposable, IUdpTx
    {
        private bool is_disposed = false;
        private bool transporting = false;
        private int state { get; set; }
        public IPEndPoint RemoteIP { get; private set; }
        private IEndTransport EndTransport { get; set; }
        private ITxEvents events { get; set; }
        private IAsyncResult AsyncResult { get; set; }
        protected TaskFactory Task { get; set; }
        private byte[] buffer { get; set; }
        private int offset { get; set; }
        private int length { get; set; }
        protected readonly CompositeDisposable disposables;
        public UdpTx()
        {
            this.disposables = new CompositeDisposable();
            this.Task = new TaskFactory();
            this.TransportBufferSize = Consts.TransportBytes;
            this.TransportedObservable = Observable.FromEvent<TransportedHandler, IUdpTx>(handler => tx => handler(tx), evt => this.Transported += evt, evt => this.Transported -= evt);

        }
        /// <summary>
        /// Gets or sets 发送完成.
        /// </summary>
        public virtual IObservable<IUdpTx> TransportedObservable { get; protected set; }

        /// <summary>
        /// 发送完成.
        /// </summary>
        /// <param name="sender">发送者.</param>
        public delegate void TransportedHandler(IUdpTx sender);

        /// <summary>
        /// 发送完成.
        /// </summary>
        public virtual event TransportedHandler Transported;

        /// <summary>
        /// Gets 发送数据块大小.
        /// </summary>
        public virtual int TransportBufferSize { get; private set; }

        /// <summary>
        /// Gets a value indicating whether 正在发送.
        /// </summary>
        public virtual bool Transporting => this.transporting;

        public virtual CompositeDisposable Disposables => this.disposables;

        public bool IsDisposed => this.is_disposed;

        public bool Running => this.Transporting;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            this.Disposables.Dispose();
            this.EndTransport = null;
            this.events?.OnDisposed(this);
        }
        public void SetRemote(IPEndPoint endPoint)
        {
            this.RemoteIP = endPoint;
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
        public virtual void StartTransport(byte[] data, int offset, int length)
        {
            this.buffer = data;
            this.offset = offset;
            this.length = length;

            this.retart_transport();
        }
        protected virtual bool to_start()
        {
            lock (this)
            {
                if (this.transporting) return false;
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

        protected virtual void retart_transport()
        {
            if (this.is_disposed)
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

        protected virtual void OnTransported()
        {
            tell_transported();
        }

        /// <summary>
        /// 发送数据.
        /// </summary>
        /// <param name="data">数据.</param>
        private void x_Transport()
        {
            try
            {
                this.AsyncResult = this.Core.BeginSendTo(this.buffer, this.offset, this.length, this.RemoteIP, this.SendCallback);
                if (this.AsyncResult != null)
                {
                    this.AsyncResult.Wait();
                    return;
                }
            }
            //已经释放了
            catch when (this.Core.IsClosed) { Debug.Assert(false); }
            catch (SocketException e)
            {
                if (!NetPsSocketException.Deal(e, this.Core, NetPsSocketExceptionSource.Write)) this.Core.ThrowException(e);
            }
            catch (Exception e) { this.Core.ThrowException(e); }
            to_end();
        }

        private void SendCallback(IAsyncResult asyncResult)
        {
            AsyncResult = null;
            try
            {
                this.state = this.Core.EndSendTo(asyncResult);
                if (this.state > 0)
                {
                    //传输完成
                    this.OnTransported();
                    return;
                }
            }
            //实例释放
            catch when (this.Core.IsClosed) { Debug.Assert(false); }
            catch (SocketException e)
            {
                if (!NetPsSocketException.Deal(e, this.Core, NetPsSocketExceptionSource.Write)) this.Core.ThrowException(e);
            }
            catch (Exception e) { this.Core.ThrowException(e); }
            to_end();
        }

        public void LookEndTransport(IEndTransport endTransport)
        {
            this.EndTransport = endTransport;
        }

        public void BindEvents(ITxEvents events)
        {
            this.events = events;
        }

        public void AddDispose(IDisposable disposable)
        {
            this.Disposables.Add(disposable);
        }

        public void SetTransportBufferSize(int size)
        {
            this.TransportBufferSize = size;
        }
    }
}
