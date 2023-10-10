namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
#if NET35_CF
    using Array = System.Array2;
#endif

    /// <summary>
    /// tcp 接收控制.
    /// </summary>
    public class TcpRx : BindTcpCore, IDisposable, ITcpRx
    {
        private bool is_disposed = false;
        public int nReceived { get; protected set; }
        protected byte[] bBuffer { get; private set; }
        protected TaskFactory Task { get; private set; }
        private int nBuffersize { get; set; }
        private ITcpReceive receive { get; set; }
        private IAsyncResult AsyncResult { get; set; }
        private IRxEvents events { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpRx"/> class.
        /// </summary>
        public TcpRx()
        {
            this.Task = new TaskFactory(TaskScheduler.Default);
            this.nReceived = 0;
            this.nBuffersize = Consts.ReceiveBytes;
            this.bBuffer = new byte[this.nBuffersize];
            this.ReceivedObservable = Observable.FromEvent<ReceivedStreamHandler, byte[]>(handler => buffer => handler(buffer), evt => this.Received += evt, evt => this.Received -= evt);
        }

        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<byte[]> ReceivedObservable { get; protected set; }

        /// <summary>
        /// 接收流.
        /// </summary>
        /// <param name="stream">流.</param>
        public delegate void ReceivedStreamHandler(byte[] stream);

        /// <summary>
        /// 接收数据.
        /// </summary>
        public virtual event ReceivedStreamHandler Received;
        public bool Running => this.Core.Receiving;
        /// <summary>
        /// 缓冲区
        /// </summary>
        public virtual byte[] Buffer => bBuffer;

        /// <summary>
        /// 接收数据大小
        /// </summary>
        public virtual int ReceivedSize => this.nReceived;

        /// <summary>
        /// Gets 接送缓冲区大小.
        /// </summary>
        public virtual int BufferSize => this.nBuffersize;

        public virtual void WhenReceived(ITcpReceive tcp_receive)
        {
            this.receive = tcp_receive;
        }
        /// <summary>
        /// 开始接收.
        /// </summary>
        public virtual void StartReceive()
        {
            if (this.is_disposed) return;
            lock (this.Core)
            {
                if (this.Core.Receiving) return;
                this.Core.Receiving = true;
            }

            this.events?.OnReceiving(this);
            this.restart_receive();
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
                if (this.Core != null) this.Core.Receiving = false;
            }
            this.events?.OnDisposed(this);
            this.bBuffer = null;
        }

        /// <summary>
        /// 发送接收事件
        /// </summary>
        /// <param name="data"></param>
        public virtual void SendReceived(byte[] data)
        {
            if (data != null && data.Length > 0)
            {
                if (this.receive != null) receive.TcpReceive(data, this.Core);
                if (this.Received != null) this.Received.Invoke(data);
            }
        }

        protected virtual void OnReceived()
        {
            if (this.is_disposed) return;
            var data = new byte[this.nReceived];
            Array.Copy(this.bBuffer, data, this.nReceived);
            SendReceived(data);
            restart_receive();
        }

        protected void restart_receive() 
        {
            if (this.Core.IsClosed) return;
            try
            {
                Task.StartNew(this.x_StartReceive);
            }
            catch { Debug.Assert(false); }
        }

        private void x_StartReceive()
        {
            if (this.Core.IsClosed) return;

            try
            {
                this.AsyncResult = this.Core.BeginReceive(this.bBuffer, 0, this.nBuffersize, this.ReceiveCallback);
                if (this.AsyncResult != null)
                {
                    this.AsyncResult.Wait();
                    return;
                }
            }
            catch when (this.Core.IsClosed) { Debug.Assert(false); }
            catch (SocketException e)
            {
                Debug.Assert(false);
                AsyncResult = null;
                if (!NetPsSocketException.Deal(e, this.Core, NetPsSocketExceptionSource.Read)) this.Core.ThrowException(e);
            }
            catch (Exception e) { this.Core.ThrowException(e); }

            this.Core.Receiving = false;
            if (!this.Core.IsClosed) this.Core.Lose();
        }
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            AsyncResult = null;
            try
            {
                //socket closed, last result.
                this.nReceived = this.Core.EndReceive(asyncResult);
                if (this.nReceived > 0)
                {
                    Debug.Assert(this.Core.Socket.Connected);
                    this.events?.OnReceived(this);
                    this.OnReceived();
                    return;
                }
            }
            catch when (this.Core.IsClosed) { Debug.Assert(false); }
            catch (SocketException e)
            {
                Debug.Assert(false);
                this.nReceived = 0;
                if (!NetPsSocketException.Deal(e, this.Core, NetPsSocketExceptionSource.Read)) this.Core.ThrowException(e);
            }
            catch (Exception e) { this.Core.ThrowException(e); }

            this.Core.Receiving = false;
            if (!this.is_disposed) this.Core.Lose();
        }

        public void BindEvents(IRxEvents events)
        {
            this.events = events;
        }
    }
}
