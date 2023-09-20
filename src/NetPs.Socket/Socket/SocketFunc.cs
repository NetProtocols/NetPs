namespace NetPs.Socket
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// 处理异常.
    /// </summary>
    /// <param name="exception">异常.</param>
    public delegate void SocketExceptionHandler(Exception exception);
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
        public abstract bool Actived { get; }
        public abstract bool IsShutdown { get; }
        public virtual bool IsSocketClosed { get; private set; } = false;
        public event EventHandler SocketClosed;
        /// <summary>
        /// 处理异常.
        /// </summary>
        public virtual event SocketExceptionHandler SocketException;
        /// <summary>
        /// 抛出异常.
        /// </summary>
        /// <param name="e">异常.</param>
        public virtual void ThrowException(Exception e)
        {
            Debug.Assert(!this.IsClosed);
            this.SocketException?.Invoke(e);
        }

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
                try
                {
                    //防止未初始化socket的情况
                    this.IsSocketClosed = true;
                    if (SocketClosed != null) SocketClosed.Invoke(this, null);
                    Thread.Sleep(1000);
                    this.Socket.Close();
                }
                catch
                {
                    Debug.Assert(false);
                }
            }
        }
        public virtual void WaitHandle(IAsyncResult asyncResult)
        {
            try
            {
                if (!asyncResult.IsCompleted)
                {
                    asyncResult.AsyncWaitHandle.WaitOne();
                    asyncResult.AsyncWaitHandle.Close();
                }
                else
                {
                    asyncResult.AsyncWaitHandle.Close();
                }
            }
            catch when (this.IsClosed) { Debug.Assert(false); }
            catch (Exception e) { this.ThrowException(e); }
        }

        protected virtual void socket_shutdown(SocketShutdown how)
        {
            if (this.Socket != null && this.Socket.ProtocolType == ProtocolType.Tcp)
            {
                try
                {
                    if (this.Socket.Connected)
                    {
                        this.Socket.Shutdown(how);
                    }
                }
                catch when (this.IsClosed) { Debug.Assert(false); }
                catch (Exception e) { this.ThrowException(e); }
            }
        }
        public virtual bool PollRead(int millisecondsTimeout)
        {
            try
            {
                return !this.IsClosed && this.Socket.Poll(millisecondsTimeout, SelectMode.SelectRead);
            }
            catch
            {
                return false;
            }
        }
        public virtual bool PollWrite(int millisecondsTimeout)
        {
            try
            {

                return !this.IsClosed && this.Socket.Poll(millisecondsTimeout, SelectMode.SelectWrite);
            }
            catch
            {
                return false;
            }
        }
        public virtual bool PollError(int millisecondsTimeout)
        {
            return !this.IsClosed && this.Socket.Poll(millisecondsTimeout, SelectMode.SelectError);
        }
        public virtual IAsyncResult BeginAccept(AsyncCallback callback)
        {
            if (!this.Actived) return null;
            return this.Socket.BeginAccept(callback, null);
        }
        public virtual Socket EndAccept(IAsyncResult asyncResult)
        {
            if (!this.Actived) return null;
            return this.Socket.EndAccept(asyncResult);
        }
        public virtual IAsyncResult BeginConnect(IPEndPoint address, AsyncCallback callback)
        {
            if (this.IsClosed) return null;
            return this.Socket.BeginConnect(address, callback, null);
        }
        public virtual void EndConnect(IAsyncResult asyncResult)
        {
            if (this.IsClosed) return;
            this.Socket.EndConnect(asyncResult);
        }
        public virtual IAsyncResult BeginReceive(byte[] buffer, int offset, int count, AsyncCallback callback)
        {
            if (buffer == null || this.IsClosed) return null;
            if (this.Socket.Available <= 0 && this.PollRead(0)) return null;
            if (!this.Actived) return null;
            return this.Socket.BeginReceive(buffer, offset, count, SocketFlags.None, callback, null);
        }
        public virtual int EndReceive(IAsyncResult asyncResult)
        {
            if (!this.Actived) return 0;
            return this.Socket.EndReceive(asyncResult);
        }
        public virtual IAsyncResult BeginSend(byte[] buffer, int offset, int count, AsyncCallback callback)
        {
            if (buffer == null || !this.Actived) return null;
            if (this.IsClosed) return null;
            return this.Socket.BeginSend(buffer, offset, count, SocketFlags.None, callback, null);
        }
        public virtual int EndSend(IAsyncResult asyncResult)
        {
            if (!this.Actived) return 0;
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
