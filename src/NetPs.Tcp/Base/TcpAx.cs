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
            lock(this)
            this.is_disposed = true;
            this.Accepted = null;
        }

        /// <summary>
        /// 开始接受Client.
        /// </summary>
        public virtual void StartAccept()
        {
            if (this.core.IsDisposed) return;
            try
            {
                lock (this)
                {
                    if (this.is_disposed) return;
                }
                if (this.core.Socket == null) this.core.OnLoseConnected();
                else this.core.Socket.BeginAccept(this.AcceptCallback, null);
            }
            catch (NullReferenceException)
            {
                //释放
            }
            catch (SocketException e)
            {
                if (this.core != null && this.core.Actived) StartAccept(); //忽略 客户端连接错误
                else if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Accept)) this.core.ThrowException(e);
            }
        }
        private void AcceptCallback(IAsyncResult asyncResult)
        {
            try
            {
                Socket client = null;
                lock (this)
                {
                    if (this.is_disposed) return;
                    client = this.core.Socket.EndAccept(asyncResult);
                }
                asyncResult.AsyncWaitHandle.Close();
                this.StartAccept();
                Task.Factory.StartNew(tell_accept, client);
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException)
            {
                //释放
            }
            catch (SocketException e)
            {
                if (this.core != null && this.core.Actived) StartAccept(); //忽略 客户端连接错误
                //请求错误不处理
                else if (!NetPsSocketException.Deal(e, this.core, NetPsSocketExceptionSource.Accept)) this.core.ThrowException(e);
            }
        }
        private void tell_accept(object client)
        {
            if (this.Accepted != null) Accepted.Invoke(client as Socket);
        }
    }
}
