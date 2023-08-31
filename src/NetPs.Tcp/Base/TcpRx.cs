﻿namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading;
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
        protected CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpRx"/> class.
        /// </summary>
        /// <param name="tcpCore">.</param>
        public TcpRx(TcpCore tcpCore)
        {
            this.CancellationToken = new CancellationToken();
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
            if (this.is_disposed) return;
            lock (this.core)
            {
                if (this.core.Receiving) return;
                this.core.Receiving = true;
            }

            this.restart_receive();
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
                this.core.Receiving = false;
            }
            CancellationToken.WaitHandle.Close();
        }

        /// <summary>
        /// 发送接收事件
        /// </summary>
        /// <param name="data"></param>
        public virtual void SendReceived(byte[] data)
        {
            if (this.Received != null && data != null && data.Length > 0) this.Received.Invoke(data);
        }

        public virtual void OnRecevied()
        {
            var data = new byte[this.nReceived];
            Array.Copy(this.bBuffer, data, this.nReceived);
            SendReceived(data);
            Task.Factory.StartNew(restart_receive, CancellationToken);
        }

        protected void restart_receive() => this.x_StartReceive();

        private void x_StartReceive()
        {
            if (this.is_disposed) return;

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
                if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Read)) this.core.ThrowException(e);
            }
        }
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            if (this.is_disposed) return;
            try
            {
                this.nReceived = this.core.Socket.EndReceive(asyncResult);
                asyncResult.AsyncWaitHandle.Close();
                if (this.nReceived <= 0)
                {
                    this.core.Lose();
                    return;
                }
                this.OnRecevied();
                return;
            }
            //释放
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException e)
            {
                this.nReceived = 0;

                if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Read)) this.core.ThrowException(e);
            }
        }

    }
}
