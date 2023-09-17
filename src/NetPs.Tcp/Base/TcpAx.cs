namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
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
                SocketCore.WaitHandle(AsyncResult, () => { });
                //{
                //    this.Core.Socket.EndAccept(AsyncResult);
                //});
                this.AsyncResult = null;
            }
            this.Accepted = null;
        }

        /// <summary>
        /// 开始接受Client.
        /// </summary>
        public virtual void StartAccept() => Task.StartNew(this.start_accept);
        private void start_accept()
        {
            if (this.is_disposed) return;
            try
            {
                AsyncCallback = new AsyncCallback(this.AcceptCallback);
                AsyncResult = this.Core.Socket.BeginAccept(AsyncCallback, null);
                return;
            }
            catch (ObjectDisposedException) { return; }
            catch (NullReferenceException) { return; }
            catch (SocketException)
            {
                //忽略 客户端连接错误
                if (this.is_disposed) return;
            }

            StartAccept();
        }
        private void AcceptCallback(IAsyncResult asyncResult)
        {
            if (this.Core.IsDisposed) return;
            try
            {
                Socket client = null;
                client = this.Core.Socket.EndAccept(asyncResult);
                asyncResult.AsyncWaitHandle.Close();
                StartAccept();
                Task.StartNew(tell_accept, client);
                return;
            }
            catch (ObjectDisposedException) { return; }
            catch (NullReferenceException) { return; }
            catch (SocketException)
            {
                //请求错误不处理
                if (this.is_disposed) return;
            }
            StartAccept();
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
