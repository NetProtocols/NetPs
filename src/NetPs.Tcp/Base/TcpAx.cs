namespace NetPs.Tcp
{
    using System;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// 服务.
    /// </summary>
    public class TcpAx : IDisposable
    {
        private bool is_disposed = false;
        private TcpCore core { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpAx"/> class.
        /// </summary>
        /// <param name="tcpCore">.</param>
        public TcpAx(TcpCore tcpCore)
        {
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
            this.Accepted = null;
        }

        /// <summary>
        /// 开始接受Client.
        /// </summary>
        public virtual void StartAccept()
        {
            if (this.is_disposed) return;
            try
            {
                this.core.Socket.BeginAccept(this.AcceptCallback, null);
                return;
            }
            catch (ObjectDisposedException) { return; }
            catch (NullReferenceException) { return; }
            catch (SocketException e)
            {
                //忽略 客户端连接错误
                if (this.is_disposed) return;
            }

            this.StartAccept();
        }
        private void AcceptCallback(IAsyncResult asyncResult)
        {
            if (this.is_disposed) return;
            try
            {
                Socket client = null;
                client = this.core.Socket.EndAccept(asyncResult);
                asyncResult.AsyncWaitHandle.Close();
                Task.Factory.StartNew(tell_accept, client);
            }
            catch (ObjectDisposedException) { return; }
            catch (NullReferenceException) { return; }
            catch (SocketException e)
            {
                //请求错误不处理
                if (this.is_disposed) return;
            }
            this.StartAccept();
        }
        private void tell_accept(object client)
        {
            if (this.Accepted != null) Accepted.Invoke(client as Socket);
        }
    }
}
