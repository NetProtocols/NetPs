namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
#if NET35_CF
    using Array = System.Array2;
#endif

    /// <summary>
    /// tcp 接收控制.
    /// </summary>
    public class TcpRx : IDisposable
    {
        protected TcpCore core { get; }
        protected byte[] bBuffer { get; private set; }
        public int nReceived { get; protected set; }
        private int nBuffersize { get; set; }
        private bool is_disposed = false;
        public bool Running => this.core.Receiving;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpRx"/> class.
        /// </summary>
        /// <param name="tcpCore">.</param>
        public TcpRx(TcpCore tcpCore)
        {
            this.nReceived = 0;
            this.core = tcpCore;
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

        /// <summary>
        /// Gets 接送缓冲区大小.
        /// </summary>
        public virtual int ReceiveBufferSize => this.nBuffersize;
        
        /// <summary>
        /// 开始接收.
        /// </summary>
        public virtual void StartReceive()
        {
            lock (this)
            {
                if (this.core.Receiving) return;
            }
            this.core.Receiving = true;
            this.x_StartReceive();
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (this)
            {
                this.is_disposed = true;
                this.core.Receiving = false;
            }
        }

        /// <summary>
        /// 发送接收事件
        /// </summary>
        /// <param name="data"></param>
        public virtual void SendReceived(byte[] data)
        {
            if (this.Received != null && data != null && data.Length > 0) this.Received.Invoke(data);
        }

        public virtual void EndRecevie()
        {
            var data = new byte[this.nReceived];
            Array.Copy(this.bBuffer, data, this.nReceived);
            SendReceived(data);
            Task.Factory.StartNew(this.x_StartReceive);
        }

        protected virtual void x_StartReceive()
        {
            lock (this)
            {
                if (this.is_disposed || this.core.Socket == null) return;
                else if (!this.core.Actived)
                {
                    this.core.OnLoseConnected();
                    return;
                }
            }

            try
            {
                this.core.Socket.BeginReceive(this.bBuffer, 0, this.nBuffersize, SocketFlags.None, this.ReceiveCallback, null);
                return;
            }
            //释放
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException e)
            {
                if (this.core == null || !this.core.Actived) this.core.OnLoseConnected();
                else if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Read)) this.core.ThrowException(e);
            }
        }
        private void ReceiveCallback(IAsyncResult asyncResult)
        {

            try
            {
                lock (this)
                {
                    if (this.is_disposed) return;
                    this.nReceived = this.core.Socket.EndReceive(asyncResult);
                    asyncResult.AsyncWaitHandle.Close();
                    if (this.nReceived <= 0)
                    {
                        this.core.OnLoseConnected();
                        return;
                    }
                }
                this.EndRecevie();
                return;
            }
            //释放
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException e)
            {
                this.nReceived = 0;

                if (this.core == null || !this.core.Actived) this.core.OnLoseConnected();
                else if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Read)) this.core.ThrowException(e);
            }
        }

    }
}
