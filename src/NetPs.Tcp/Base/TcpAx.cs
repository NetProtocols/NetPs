namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// 服务.
    /// </summary>
    public class TcpAx : IDisposable, IBindTcpCore
    {
        private bool is_disposed = false;
        protected TaskFactory Task { get; private set; }
        private IAsyncResult AsyncResult { get; set; }
        private AsyncCallback AsyncCallback { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpAx"/> class.
        /// </summary>
        /// <param name="tcpCore">.</param>
        public TcpAx()
        {
            this.Task = new TaskFactory();
            this.AsyncCallback = new AsyncCallback(this.AcceptCallback);
            this.AcceptObservable = Observable.FromEvent<TcpAx.AcceptSocketHandler, Socket>(handler => socket => handler(socket), evt => this.Accepted += evt, evt => this.Accepted -= evt);
        }

        /// <summary>
        /// Gets or sets 接受.
        /// </summary>
        public virtual IObservable<Socket> AcceptObservable { get; protected set; }

        public TcpCore Core { get; private set; }

        /// <summary>
        /// 接受Socket.
        /// </summary>
        /// <param name="socket">套接字.</param>
        public delegate void AcceptSocketHandler(Socket socket);

        /// <summary>
        /// 接受Socket.
        /// </summary>
        public virtual event AcceptSocketHandler Accepted;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            if (AsyncResult != null)
            {
                this.Core.WaitHandle(AsyncResult);
                //{
                //    this.Core.Socket.EndAccept(AsyncResult);
                //});
                this.AsyncResult = null;
            }
        }

        /// <summary>
        /// 开始接受Client.
        /// </summary>
        public virtual async void StartAccept()
        {
            if (!this.Core.IsClosed)
            {
                try
                {
                    await Task.StartNew(this.start_accept);
                }
                catch
                {
                    Debug.Assert(false);
                }
            }
        }
        private void start_accept()
        {
            if (this.Core.IsClosed) return;
            try
            {
                AsyncResult = this.Core.BeginAccept(AsyncCallback);
                AsyncResult.Wait();
                return;
            }
            catch when (this.Core.IsClosed) { Debug.Assert(false); }
            catch (SocketException)
            {
                //忽略 客户端连接错误
                if (this.Core.IsDisposed) return;
            }
            catch (Exception e) { this.Core.ThrowException(e); }

            StartAccept();
        }
        private void AcceptCallback(IAsyncResult asyncResult)
        {
            try
            {
                var client = this.Core.EndAccept(asyncResult);
                asyncResult.AsyncWaitHandle.Close();
                if (this.Core.IsClosed) return;
                StartAccept();
                if (client != null)
                {
                    Task.StartNew(tell_accept, client);
                }
                return;
            }
            catch when (this.Core.IsClosed) { Debug.Assert(false); }
            catch (SocketException)
            {
                Debug.Assert(false);
                //请求错误不处理
                if (this.is_disposed) return;
                StartAccept();
            }
            catch (Exception e) { this.Core.ThrowException(e); }
        }
        private void tell_accept(object client)
        {
            if (this.Accepted != null) Accepted.Invoke(client as Socket);
        }

        public void BindCore(TcpCore core)
        {
            this.Core = core;
        }
    }
}
