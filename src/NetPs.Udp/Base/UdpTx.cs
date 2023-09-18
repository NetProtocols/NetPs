namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class UdpTx : IDisposable, ITx
    {
        public UdpCore core { get; set; }
        private bool is_disposed = false;
        private bool transporting = false;
        private int state { get; set; }
        public IPEndPoint RemoteIP { get; }
        private IEndTransport EndTransport { get; set; }
        private ITxEvents events { get; set; }
        private IAsyncResult AsyncResult { get; set; }
        protected TaskFactory Task { get; set; }
        private byte[] buffer { get; set; }
        private int offset { get; set; }
        private int length { get; set; }
        protected readonly CompositeDisposable disposables;
        internal UdpTx()
        {
            this.disposables = new CompositeDisposable();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UdpTx"/> class.
        /// </summary>
        /// <param name="core">.</param>
        public UdpTx(UdpCore udpCore, IPEndPoint endPoint)
        {
            this.disposables = new CompositeDisposable();
            this.Task = new TaskFactory();
            this.core = udpCore;
            this.RemoteIP = endPoint;
            this.TransportBufferSize = Consts.TransportBytes;
            this.TransportedObservable = Observable.FromEvent<TransportedHandler, UdpTx>(handler => tx => handler(tx), evt => this.Transported += evt, evt => this.Transported -= evt);
        }

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
        /// Gets 发送数据块大小.
        /// </summary>
        public virtual int TransportBufferSize { get; }

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
            if (this.AsyncResult != null)
            {
                SocketCore.WaitHandle(AsyncResult);
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
            Task.StartNew(tell_transported);
        }

        /// <summary>
        /// 发送数据.
        /// </summary>
        /// <param name="data">数据.</param>
        private void x_Transport()
        {
            try
            {
                if (this.is_disposed || this.core.IsClosed) return;
                this.AsyncResult = this.core.Socket.BeginSendTo(this.buffer, this.offset, this.length, SocketFlags.None, this.RemoteIP, this.SendCallback, null);
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

        private void SendCallback(IAsyncResult asyncResult)
        {
            if (this.is_disposed || this.core.IsClosed) return;
            AsyncResult = null;
            try
            {
                this.state = this.core.Socket.EndSendTo(asyncResult);
                asyncResult.AsyncWaitHandle.Close();
                //传输完成
                this.OnTransported();
                return;
            }
            //实例释放
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException e)
            {
                if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Write)) this.core.ThrowException(e);
            }
        }

        public void LookEndTransport(IEndTransport endTransport)
        {
            this.EndTransport = endTransport;
        }

        public void BindEvents(ITxEvents events)
        {
            this.events = events;
        }
    }
}
