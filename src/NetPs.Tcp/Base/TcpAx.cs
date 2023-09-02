namespace NetPs.Tcp
{
    using System;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 服务.
    /// </summary>
    public class TcpAx : IDisposable
    {
        private bool is_disposed = false;
        private TcpCore core { get; }
        protected TaskFactory Task { get; private set; }
        private IAsyncResult AsyncResult { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpAx"/> class.
        /// </summary>
        /// <param name="tcpCore">.</param>
        public TcpAx(TcpCore tcpCore)
        {
            this.Task = new TaskFactory();
            this.core = tcpCore;
            this.AcceptObservable = Observable.FromEvent<TcpAx.AcceptSocketHandler, Socket>(handler => socket => handler(socket), evt => this.Accepted += evt, evt => this.Accepted -= evt);
        }

        /// <summary>
        /// Gets or sets 接受.
        /// </summary>
        public virtual IObservable<Socket> AcceptObservable { get; protected set; }

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
                //必要的end操作
                this.core.Socket.EndAccept(AsyncResult);
                AsyncResult.AsyncWaitHandle.Close();
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
                AsyncResult = this.core.Socket.BeginAccept(this.AcceptCallback, null);
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
            if (this.is_disposed) return;
            try
            {
                Socket client = null;
                client = this.core.Socket.EndAccept(asyncResult);
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
    }
}
