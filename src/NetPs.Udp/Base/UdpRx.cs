namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Runtime.Serialization;

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
        private readonly UdpCore core;

        private byte[] bBuffer;

        private int nReceived;

        private int nBuffersize;

        private bool isDisposed = false;

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
            this.ReceicedObservable = Observable.FromEvent<ReveicedStreamHandler, UdpData>(handler => data => handler(data), evt => this.Reveiced += evt, evt => this.Reveiced -= evt);
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
        public virtual event ReveicedStreamHandler Reveiced;

        /// <summary>
        /// Gets 接送缓冲区大小.
        /// </summary>
        public virtual int ReceiveBufferSize => this.nBuffersize;

        /// <summary>
        /// 开始接收.
        /// </summary>
        public virtual void StartReveice()
        {
            this._StartReveice();
            this.core.Receiving = true;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            this.isDisposed = true;
            this.Reveiced = null;
            this.core.Receiving = false;
        }

        private void _StartReveice()
        {
            if (this.isDisposed)
            {
                return;
            }

            try
            {
                this.core.Socket?.BeginReceiveFrom(this.bBuffer, 0, this.nBuffersize, SocketFlags.None, ref remotePoint, this.ReceiveCallback, this.core.Socket);
            }
            catch (SocketException e)
            {
                var ex = new NetPsSocketException(e, this.core);
                if (!ex.Handled)
                {
                    throw ex;
                }
            }
        }

        private void ReceiveCallback(IAsyncResult asyncResult)
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
                    this.nReceived = client.EndReceiveFrom(asyncResult, ref this.remotePoint);
                    asyncResult.AsyncWaitHandle.Close();
                }
                catch (SocketException e)
                {
                    this.nReceived = -1;
                    var ex = new NetPsSocketException(e, this.core);
                    if (!ex.Handled)
                    {
                        this.core.ThrowException(e);
                    }
                }

                if (this.nReceived > 0)
                {
                    var buffer = new byte[this.nReceived];
                    Array.Copy(this.bBuffer, 0, buffer, 0, this.nReceived);
                    Array.Clear(this.bBuffer, 0, this.nReceived);
                    this.Reveiced?.Invoke(new UdpData { Data = buffer, IP = this.remotePoint as IPEndPoint });
                }
                this._StartReveice();

            }
            catch (Exception e)
            {
                this.core.ThrowException(e);
            }
        }
    }
}
