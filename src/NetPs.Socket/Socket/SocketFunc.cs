namespace NetPs.Socket
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public abstract class SocketFunc
    {
        /// <summary>
        /// Gets or sets tcp client.
        /// </summary>
        public virtual Socket Socket { get; protected set; }
        /// <summary>
        /// Gets or sets 地址.
        /// </summary>
        public virtual ISocketUri Address { get; protected set; }
        /// <summary>
        /// Gets or sets 终端地址.
        /// </summary>
        public virtual IPEndPoint IPEndPoint { get; protected set; }
        public abstract bool IsDisposed { get; }
        public abstract bool IsClosed { get; }
        /// <summary>
        /// 复用地址
        /// </summary>
        /// <remarks>
        /// * 需要管理员权限
        /// <br/>！这个范围被约束在同一进程并且配置复用的情况下。
        /// <br/><br/>用于多个socket 使用相同的地址端口绑定的情况。
        /// </remarks>
        public virtual void SetReuseAddress(bool optionValue)
        {
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue);

        }
        /// <summary>
        /// Linger
        /// </summary>
        /// <remarks>
        /// false,0 Close 时快速断开不发送fin
        /// </remarks>
        /// <param name="enable"></param>
        /// <param name="seconds"></param>
        public virtual void SetLinger(bool enable, int seconds)
        {
            var optionValue = new LingerOption(enable, seconds);
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, optionValue);
        }
        public virtual void SetFastOpen()
        {
            this.Socket.SetSocketOption(SocketOptionLevel.Tcp, (SocketOptionName)15, true);
        }
        public virtual void SetSendTimeout(int timeout)
        {
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout);
        }
        protected virtual Socket new_socket()
        {
            switch (Address.Scheme)
            {
                case InsideSocketUri.UriSchemeTCP:
                    return new Socket(this.Address.IP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                case InsideSocketUri.UriSchemeUDP:
                    return new Socket(this.Address.IP.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                case InsideSocketUri.UriSchemeICMP:
                    if (Address.IsIpv4())
                    {
                        return new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
                    }
                    else
                    {
#if NET35_CF
                        return new System.Net.Sockets.Socket(AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.Icmp);
#else
                        return new System.Net.Sockets.Socket(AddressFamily.InterNetworkV6, SocketType.Raw, ProtocolType.IcmpV6);
#endif
                    }
            }
            return null;
        }
        protected virtual void close_socket()
        {
            if (this.Socket != null)
            {
                //防止未初始化socket的情况
                this.Socket.Close();
                //if (this.Socket is IDisposable o) o.Dispose();
                //this.Socket = null;
            }
        }
        protected virtual void socket_shutdown(SocketShutdown how)
        {
            if (this.Socket != null)
            {
                try
                {
                    this.Socket.Shutdown(how);
                }
                catch (ObjectDisposedException) { }
                catch (SocketException) { }
            }
        }
        public virtual bool PollRead(int millisecondsTimeout)
        {
            return !this.IsDisposed && this.Socket.Poll(millisecondsTimeout, SelectMode.SelectRead);
        }
        public virtual bool PollWrite(int millisecondsTimeout)
        {
            return !this.IsDisposed && this.Socket.Poll(millisecondsTimeout, SelectMode.SelectWrite);
        }
        public virtual IAsyncResult BeginAccept(AsyncCallback callback)
        {
            if (this.IsDisposed) return null;
            return this.Socket.BeginAccept(callback, null);
        }
        public virtual Socket EndAccept(IAsyncResult asyncResult)
        {
            if (this.IsDisposed) return null;
            return this.Socket.EndAccept(asyncResult);
        }
        public virtual IAsyncResult BeginConnect(IPEndPoint address, AsyncCallback callback)
        {
            if (this.IsDisposed) return null;
            return this.Socket.BeginConnect(address, callback, null);
        }
        public virtual void EndConnect(IAsyncResult asyncResult)
        {
            if (this.IsDisposed) return;
            this.Socket.EndConnect(asyncResult);
        }
        public virtual IAsyncResult BeginReceive(byte[] buffer, int offset, int count, AsyncCallback callback)
        {
            if (buffer == null ||this.IsDisposed) return null;
            return this.Socket.BeginReceive(buffer, offset, count, SocketFlags.None, callback, null);
        }
        public virtual int EndReceive(IAsyncResult asyncResult)
        {
            if (this.IsDisposed) return 0;
            return this.Socket.EndReceive(asyncResult);
        }
        public virtual IAsyncResult BeginSend(byte[] buffer, int offset, int count, AsyncCallback callback)
        {
            if (buffer == null ||this.IsDisposed) return null;
            return this.Socket.BeginSend(buffer, offset, count, SocketFlags.None, callback, null);
        }
        public virtual int EndSend(IAsyncResult asyncResult)
        {
            if (this.IsDisposed) return 0;
            return this.Socket.EndSend(asyncResult);
        }

        public virtual IPEndPoint CreateIPAny()
        {
            if (Address.IsIpv6())
            {
                return new IPEndPoint(IPAddress.IPv6Any, 0);
            }
            return new IPEndPoint(IPAddress.Any, 0);
        }
    }
}
