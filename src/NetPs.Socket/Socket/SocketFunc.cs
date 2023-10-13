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
        private bool is_shutdown = false;
        /// <summary>
        /// Gets or sets tcp client.
        /// </summary>
        public virtual Socket Socket { get; internal set; }
        /// <summary>
        /// Gets or sets 地址.
        /// </summary>
        public virtual ISocketUri Address { get; protected set; }
        /// <summary>
        /// Gets or sets 终端地址.
        /// </summary>
        public virtual IPEndPoint IPEndPoint { get; protected set; }
        /// <summary>
        /// 远程地址
        /// </summary>
        public virtual ISocketUri RemoteAddress { get; protected set; }
        /// <summary>
        /// 远程地址
        /// </summary>
        public virtual IPEndPoint RemoteIPEndPoint { get; protected set; }
        public abstract bool IsDisposed { get; }
        public abstract bool IsClosed { get; }
        public abstract bool Actived { get; }
        public virtual bool IsShutdown => this.is_shutdown;
        public virtual bool IsSocketClosed { get; private set; } = false;
        public event EventHandler SocketClosed;
        private ManualResetEvent manualResetEvent { get; set; }
        public SocketFunc()
        {
            manualResetEvent = new ManualResetEvent(false);
        }
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
        protected virtual void SetSocket(Socket socket)
        {
            this.Socket = socket;
            if (socket.LocalEndPoint != null && socket.RemoteEndPoint != null)
            {
                var scheme = GetScheme();
                this.IPEndPoint = socket.LocalEndPoint as IPEndPoint;
                this.Address = new InsideSocketUri(scheme, this.IPEndPoint);
                this.RemoteIPEndPoint = socket.RemoteEndPoint as IPEndPoint;
                this.RemoteAddress = new InsideSocketUri(scheme, this.RemoteIPEndPoint);
            }
        }
        public virtual string GetScheme()
        {
            if (Socket == null) return InsideSocketUri.UriSchemeUnknown;
            switch (Socket.ProtocolType)
            {
                case ProtocolType.Tcp:
                    return InsideSocketUri.UriSchemeTCP;
                case ProtocolType.Udp:
                    return InsideSocketUri.UriSchemeUDP;
                case ProtocolType.Icmp:
#if NETSTANDARD
                case ProtocolType.IcmpV6:
#endif
                    return InsideSocketUri.UriSchemeICMP;
                default:
                    return InsideSocketUri.UriSchemeUnknown;
            }
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
        public virtual bool GetReuseAddress()
        {
            var value = this.Socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress);
            if (value != null && (value == (object)1 || value == (object)true))
            {
                return true;
            }
            return false;
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
        public virtual int GetSendTimeout()
        {
            var value = this.Socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
            if (value is int val) return val;
            return 0;
        }
        public virtual void SetBroadcast(bool value)
        {
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, value);
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
            //防止未初始化socket的情况
            if (this.Socket != null)
            {
                try
                {
                    this.IsSocketClosed = true;
                    this.BlockingDelay(300);
                    this.Socket.Close();
                    GC.WaitForPendingFinalizers();
                    this.BlockingDelay(300);
                    if (SocketClosed != null) SocketClosed.Invoke(this, null);
                }
                catch
                {
                    Debug.Assert(false);
                }
            }
        }

        protected virtual void socket_shutdown(SocketShutdown how)
        {
            if (!this.Address.IsTcp()) throw new ArgumentException("cannot shutdown because not TCP");
            //限制每个SocketCore实例只能shutdown 一次
            lock (this)
            {
                if (this.is_shutdown) return;
                this.is_shutdown = true;
            }

            if (this.Socket != null)
            {
                try
                {
                    /**
                     * 已经丢失的连接，发送FIN包已经毫无意义，并且会触发异常。
                     */
                    if (this.Socket.Connected)
                    {
                        this.BlockingDelay(50);
                        if (!this.Socket.Connected) return;
                        this.Socket.Shutdown(how);
                    }
                }
                catch when (this.IsClosed) { Debug.Assert(false); }
                catch (Exception e) { this.ThrowException(e); }
            }
        }

        internal bool BlockingDelay(int timeout)
        {
            var wait = false;
            if (this.Socket == null) return wait;

            /**
             * 当前状态为阻塞时 等待片刻。
             */
            if (this.Socket.Blocking)
            {
                this.manualResetEvent.Reset();
                do this.manualResetEvent.WaitOne(10, false); while ((timeout -= 10) > 0 && this.Socket.Connected);
                wait = true;
            }
            return wait;
        }
        public virtual bool PollRead(int millisecondsTimeout)
        {
            if (!this.Actived) return false;
            try
            {
                return this.Socket.Connected && this.Socket.Poll(millisecondsTimeout, SelectMode.SelectRead);
            }
            catch
            {
                return false;
            }
        }
        public virtual bool PollWrite(int millisecondsTimeout)
        {
            if (!this.Actived) return false;
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
            if (!this.Actived) return false;
            return !this.IsClosed && this.Socket.Poll(millisecondsTimeout, SelectMode.SelectError);
        }
        public virtual IAsyncResult BeginAccept(AsyncCallback callback)
        {
            if (!this.Actived) return null;
            return this.Socket.BeginAccept(callback, null);
        }
        public virtual Socket EndAccept(IAsyncResult asyncResult)
        {
            Socket socket = null;
            if (this.Actived)
            {
#if NETSTANDARD
                Debug.Assert(!asyncResult.AsyncWaitHandle.SafeWaitHandle.IsClosed);
                if (!asyncResult.AsyncWaitHandle.SafeWaitHandle.IsClosed)
#endif
                    socket = this.Socket.EndAccept(asyncResult);
            }
            asyncResult.Close();
            return socket;
        }
        public virtual IAsyncResult BeginConnect(IPEndPoint address, AsyncCallback callback)
        {
            if (this.IsClosed) return null;
            return this.Socket.BeginConnect(address, callback, null);
        }
        public virtual void EndConnect(IAsyncResult asyncResult)
        {
            if (!this.IsClosed)
            {
#if NETSTANDARD
                Debug.Assert(!asyncResult.AsyncWaitHandle.SafeWaitHandle.IsClosed);
                if (!asyncResult.AsyncWaitHandle.SafeWaitHandle.IsClosed)
#endif
                    this.Socket.EndConnect(asyncResult);
            }
            asyncResult.Close();
        }
        public virtual int GetAvailable()
        {
            if (!this.IsSocketClosed) return this.Socket.Available;
            return 0;
        }
        internal abstract void OnSocketLosed();
        private bool check_receive()
        {
            if (this.IsClosed) return false;
            if (!this.Socket.Connected && !this.Socket.Blocking) return false;
            if (!this.Actived) return false;
            return true;
        }
        public virtual IAsyncResult BeginReceive(byte[] buffer, int offset, int count, AsyncCallback callback)
        {
            if (buffer != null && this.check_receive())
            {
                return this.Socket.BeginReceive(buffer, offset, count, SocketFlags.None, callback, null);
            }

            this.OnSocketLosed();
            return null;
        }
        public virtual int EndReceive(IAsyncResult asyncResult)
        {
            int received = 0;
            if (this.Actived)
            {
#if NETSTANDARD
                if (!asyncResult.AsyncWaitHandle.SafeWaitHandle.IsClosed)
#endif
                received = this.Socket.EndReceive(asyncResult);
            }
            this.manualResetEvent.Set();
            if (this.Address.IsTcp())
            {
                if (received <= 0)
                {
                    if (received == 0)
                    {
                        socket_shutdown(SocketShutdown.Both);
                    }
                    this.OnSocketLosed();
                }
            }
            asyncResult.Close();

            if (! this.Actived)
            {
                received = -1;
            }
            return received;
        }
        public virtual IAsyncResult BeginSend(byte[] buffer, int offset, int count, AsyncCallback callback)
        {
            if (buffer == null || !this.Actived) return null;
            return this.Socket.BeginSend(buffer, offset, count, SocketFlags.None, callback, null);
        }
        public virtual int EndSend(IAsyncResult asyncResult)
        {
            int sended = 0;
            if (this.Actived)
            {
                sended = 1;
#if NETSTANDARD
                if (!asyncResult.AsyncWaitHandle.SafeWaitHandle.IsClosed)
#endif
                    sended = this.Socket.EndSend(asyncResult);
            }
            this.manualResetEvent.Set();
            asyncResult.Close();

            if (!this.Actived)
            {
                sended = -1;
            }
            return sended;
        }
        public virtual IAsyncResult BeginSendTo(byte[] buffer, int offset, int count, IPEndPoint endpoint, AsyncCallback callback)
        {
            if (buffer == null || !this.Actived) return null;
            //endpoint 为广播地址时所必要的设置
            this.SetBroadcast(endpoint.Address.IsBroadcast());
            return this.Socket.BeginSendTo(buffer, offset, count, SocketFlags.None, endpoint, callback, null);
        }
        public virtual int EndSendTo(IAsyncResult asyncResult)
        {
            int sended = -1;
            if (this.Actived)
            {
#if NETSTANDARD
                if (!asyncResult.AsyncWaitHandle.SafeWaitHandle.IsClosed)
#endif
                    sended = this.Socket.EndSendTo(asyncResult);
            }
            this.manualResetEvent.Set();
            asyncResult.Close();

            if (!this.Actived)
            {
                sended = -1;
            }
            return sended;
        }
        public virtual IAsyncResult BeginReceiveFrom(byte[] buffer, int offset, int count, ref EndPoint endpoint, AsyncCallback callback)
        {
            if (buffer == null || !this.Actived) return null;
            return this.Socket.BeginReceiveFrom(buffer, offset, count, SocketFlags.None, ref endpoint, callback, null);
        }
        public virtual int EndReceiveFrom(IAsyncResult asyncResult, ref EndPoint endpoint)
        {
            int received = -1;
            if (this.Actived)
            {
#if NETSTANDARD
                Debug.Assert(!asyncResult.AsyncWaitHandle.SafeWaitHandle.IsClosed);
                if (!asyncResult.AsyncWaitHandle.SafeWaitHandle.IsClosed)
#endif
                    received = this.Socket.EndReceiveFrom(asyncResult, ref endpoint);
            }
            asyncResult.AsyncWaitHandle.Close(); 
            if (!this.Actived)
            {
                received = -1;
            }
            return received;
        }
        public virtual IPEndPoint CreateIPAny()
        {
            if (Address.IsIpv6())
            {
                return new IPEndPoint(IPAddress.IPv6Any, 0);
            }
            return new IPEndPoint(IPAddress.Any, 0);
        }

        #region deal unhandled exceptions
        private static bool unhandled_exception = false;
        internal static void register_unhandled()
        {
            if (unhandled_exception) return;
            unhandled_exception = true;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#if NETSTANDARD
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
#endif
        }
        public static int exception_no = 0;
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.IsTerminating) return;
            if (e.ExceptionObject is SocketException)
            {
                Interlocked.Increment(ref exception_no);
            }
        }

#if NETSTANDARD
        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Observed) return;
            if (e.Exception.InnerException is SocketException)
            {
                //bug:0x000001
                e.SetObserved();
                Interlocked.Increment(ref exception_no);
            }
        }
#endif
        #endregion
    }
}
