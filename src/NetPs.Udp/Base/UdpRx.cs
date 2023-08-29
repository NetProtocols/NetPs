namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    [DataContract]
    public struct UdpData
    {
        [DataMember]
        public IPEndPoint IP { get; set; }
        [DataMember]
        public byte[] Data { get; set; }

    }
    public class UdpRx : IDisposable
    {
        private UdpCore core { get; set; }

        protected byte[] bBuffer { get; private set; }

        public int nReceived { get; protected set; }

        private int nBuffersize { get; set; }

        private bool is_disposed = false;

        private EndPoint remotePoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="UdpRx"/> class.
        /// </summary>
        /// <param name="udpCore">.</param>
        public UdpRx(UdpCore udpCore)
        {
            this.core = udpCore;
            this.remotePoint = new IPEndPoint(IPAddress.Any, 0);
            this.nBuffersize = Consts.ReceiveBytes;
            this.bBuffer = new byte[this.nBuffersize];
            this.ReceicedObservable = Observable.FromEvent<ReveicedStreamHandler, UdpData>(handler => data => handler(data), evt => this.Received += evt, evt => this.Received -= evt);
        }

        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<UdpData> ReceicedObservable { get; protected set; }

        /// <summary>
        /// 接收流.
        /// </summary>
        /// <param name="stream">流.</param>
        public delegate void ReveicedStreamHandler(UdpData data);

        /// <summary>
        /// 接收数据.
        /// </summary>
        public virtual event ReveicedStreamHandler Received;

        /// <summary>
        /// Gets 接送缓冲区大小.
        /// </summary>
        public virtual int ReceiveBufferSize => this.nBuffersize;

        /// <summary>
        /// 开始接收.
        /// </summary>
        public virtual void StartReveice()
        {
            this.core.Receiving = true;
            this.x_StartReveice();
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock(this)
            {
                this.is_disposed = true;
                this.core.Receiving = false;
            }
        }

        public virtual void SendReceived(byte[] data)
        {
            if (this.Received != null && data != null && data.Length > 0) this.Received.Invoke(new UdpData { Data = data, IP = this.remotePoint as IPEndPoint });
        }

        public virtual void EndRecevie()
        {
            var has_data = this.nReceived > 0;
            byte[] data = null;
            if (has_data)
            {
                data = new byte[this.nReceived];
                Array.Copy(this.bBuffer, 0, data, 0, this.nReceived);
            }
            if (has_data) SendReceived(data);
            this.x_StartReveice();
        }

        private void x_StartReveice()
        {
            try
            {
                lock (this)
                {
                    if (this.is_disposed || this.core.Socket == null) return;
                }
                this.core.Socket.BeginReceiveFrom(this.bBuffer, 0, this.nBuffersize, SocketFlags.None, ref remotePoint, this.ReceiveCallback, null);
                return;
            }
            //释放
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException e)
            {
                if (this.core.Socket == null)
                {
                    x_StartReveice(); //忽略 客户端连接错误
                    return;
                }
                if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Read)) this.core.ThrowException(e);
            }

            this.core.Receiving = false;
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                lock (this)
                {
                    if (this.is_disposed || this.core.Socket == null) return;
                    this.nReceived = this.core.Socket.EndReceiveFrom(asyncResult, ref this.remotePoint);
                }
                asyncResult.AsyncWaitHandle.Close();
                this.EndRecevie();
                return;
            }
            //释放
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException e)
            {
                this.nReceived = -1;
                if (this.core.Socket == null)
                {
                    x_StartReveice(); //忽略 客户端连接错误
                    return;
                }
                if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Read)) this.core.ThrowException(e);
            }

            this.core.Receiving = false;
        }
    }
}
