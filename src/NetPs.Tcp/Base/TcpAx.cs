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
        private readonly TcpCore core;
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
                if (this.core.Socket == null) this.core.OnLoseConnected();
                else this.core.Socket.BeginAccept(this.AcceptCallback, this.core.Socket);
            }
            catch (Exception e)
            {
                StartAccept();
                if (e is SocketException socket_e)
                {
                    var ex = new NetPsSocketException(socket_e, this.core, true);
                    if (!ex.Handled) this.core.ThrowException(ex);
                }
                else
                {
                    this.core.ThrowException(e);
                }
            }
        }
        private void AcceptCallback(IAsyncResult asyncResult)
        {
            var socket = (Socket)asyncResult.AsyncState;
            try
            {
                var client = socket.EndAccept(asyncResult);
                Task.Factory.StartNew(tell_accept, client);
                this.StartAccept();
            }
            catch (Exception e)
            {
                this.StartAccept();
                //请求错误不处理
                if (e is SocketException socket_e)
                {
                    var ex = new NetPsSocketException(socket_e, this.core, true);
                    if (!ex.Handled) this.core.ThrowException(ex);
                }
                else
                {
                    this.core.ThrowException(e);
                }
            }
            asyncResult.AsyncWaitHandle.Close();
        }
        private void tell_accept(object client)
        {
            if (this.Accepted != null) Accepted.Invoke(client as Socket);
        }
    }
}
