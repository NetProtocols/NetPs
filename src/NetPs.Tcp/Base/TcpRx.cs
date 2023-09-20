﻿namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Diagnostics;
    using System.Net;
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
    public class TcpRx : IDisposable, IRx, IBindTcpCore
    {
        protected byte[] bBuffer { get; private set; }
        public int nReceived { get; protected set; }
        private int nBuffersize { get; set; }
        private bool is_disposed = false;
        public bool Running => this.Core.Receiving;
        protected TaskFactory Task { get; private set; }
        private ITcpReceive receive { get; set; }
        private IAsyncResult AsyncResult { get; set; }
        private IRxEvents events { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpRx"/> class.
        /// </summary>
        public TcpRx()
        {
            this.Task = new TaskFactory();
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
        public virtual IPEndPoint RemoteAddress => this.Core.RemoteIPEndPoint;

        public TcpCore Core { get; private set; }

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
            //if (AsyncResult != null)
            //{
            //    this.Core.WaitHandle(AsyncResult);
            //    this.AsyncResult = null;
            //}
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
                if (this.receive != null) receive.TcpReceive(data, this.Core as TcpClient);
                if (this.Received != null) this.Received.Invoke(data);
            }
        }

        public virtual void OnRecevied()
        {
            if (this.is_disposed) return;
            var data = new byte[this.nReceived];
            Array.Copy(this.bBuffer, data, this.nReceived);
            SendReceived(data);
            restart_receive();
        }

        protected async void restart_receive() 
        {
            if (this.Core.IsClosed) return;
            try
            {
                await Task.StartNew(this.x_StartReceive);
            }
            catch { Debug.Assert(false); }
        }

        private void x_StartReceive()
        {
            if (this.Core.IsClosed) return;

            try
            {
                if (this.Core.CanBegin)
                {
                    this.AsyncResult = this.Core.BeginReceive(this.bBuffer, 0, this.nBuffersize, this.ReceiveCallback);
                    if (this.AsyncResult == null)
                    {
                        if (!this.Core.IsClosed) this.Core.Lose();
                        else this.Core.FIN();
                        return;
                    }
                    this.AsyncResult.Wait();
                }
                return;
            }
            catch when (this.Core.IsClosed) { Debug.Assert(false); }
            catch (SocketException e)
            {
                Debug.Assert(false);
                AsyncResult = null;
                if (!NetPsSocketException.Deal(e, this.Core, NetPsSocketExceptionSource.Read)) this.Core.ThrowException(e);
            }
            catch (Exception e) { this.Core.ThrowException(e); }

            if (!this.is_disposed) this.Core.Lose();
        }
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            AsyncResult = null;
            try
            {
                //socket closed, last result.
                if (this.Core.CanEnd)
                {
                    this.nReceived = this.Core.EndReceive(asyncResult);
                }
                else
                {
                    this.nReceived = 0;
                }
                asyncResult.AsyncWaitHandle.Close();
                if (this.nReceived <= 0)
                {
                    this.Dispose();
                    // 接收到 FIN
                    this.Core.FIN();
                    return;
                }
                this.events?.OnReceived(this);
                this.OnRecevied();
                return;
            }
            catch when (this.Core.IsClosed) { Debug.Assert(false); }
            catch (SocketException e)
            {
                Debug.Assert(false);
                this.nReceived = 0;
                if (!NetPsSocketException.Deal(e, this.Core, NetPsSocketExceptionSource.Read)) this.Core.ThrowException(e);
            }
            catch (Exception e) { this.Core.ThrowException(e); }
            if (!this.is_disposed) this.Core.Lose();
        }

        public void BindEvents(IRxEvents events)
        {
            this.events = events;
        }

        public void BindCore(TcpCore core)
        {
            this.Core = core;
        }
    }
}
