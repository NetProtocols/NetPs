namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net.Sockets;
    using System.Reactive.Linq;

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
            try
            {
                this.core.Socket.BeginAccept(this.AcceptCallback, this.core.Socket);
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

        private void AcceptCallback(IAsyncResult asyncResult)
        {
            try
            {
                var socket = (Socket)asyncResult.AsyncState;
                try
                {
                    var client = socket.EndAccept(asyncResult);
                    asyncResult.AsyncWaitHandle.Close();
                    this.Accepted?.Invoke(client);
                }
                catch (ObjectDisposedException)
                {
                    this.core.OnLoseConnected();
                    return;
                }
                catch (SocketException e)
                {
                    var ex = new NetPsSocketException(e, this.core);
                    if (!ex.Handled)
                    {
                        throw ex;
                    }
                }

                this.StartAccept();
            }
            catch (Exception e)
            {
                this.core.ThrowException(e);
            }
        }
    }
}
