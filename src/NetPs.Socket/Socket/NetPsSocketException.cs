namespace NetPs.Socket
{
    using System;
    using System.Net.Sockets;
    public enum NetPsSocketExceptionSource
    {
        None = 0,
        Write = 1,
        Read = 2,
        Connect = 3,
        Accept = 10,
        StartWrite = 20,
        Writing = 21,
        EndWrite = 22,
        WritingUDP = 23,
        EndWriteUDP = 24,
        ReadUDP = 33,
    }
    /// <summary>
    /// 异常处理
    /// </summary>
    public class NetPsSocketException : Exception
    {
        private bool handled { get; set; }
        private NetPsSocketExceptionSource source { get; set; }
        private SocketCore socket { get; set; }
        private string message;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetPsSocketException"/> class.
        /// </summary>
        /// <param name="errorCode">错误代码.</param>
        /// <param name="msg">消息.</param>
        /// <param name="socketException">套接字异常.</param>
        public NetPsSocketException(SocketErrorCode errorCode, string msg, SocketException socketException = null)
        {
            //this.ErrorCode = errorCode;
            this.SocketException = socketException;
            this.message = msg;
            this.source = NetPsSocketExceptionSource.None;
            handled = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetPsSocketException"/> class.
        /// </summary>
        /// <param name="e">SocketException.</param>
        /// <param name="socket">SocketCore.</param>
        public static bool Deal(SocketException e, SocketCore socket, NetPsSocketExceptionSource source)
        {
            var handled = false;
            switch ((int)e.ErrorCode)
            {
                case 0:
                    //this.ErrorCode = SocketErrorCode.Success;
                    //发送数据异常：直接发送错误，被上游设备禁止发送
                    handled = true;
                    break;
                //case 995:
                    //this.ErrorCode = SocketErrorCode.OperationAborted;
                    //由于线程退出或应用程序请求，已放弃 I/O 操作
                    //break;
                //case 997:
                    //this.ErrorCode = SocketErrorCode.IOPending;
                    //数据传输时发生 IO 重叠
                    //break;
                case 10004:
                    //this.ErrorCode = SocketErrorCode.Interrupted;
                    //调用中断，多次调用socket_close
                    handled = true;
                    break;
                //case 10013:
                //    //this.ErrorCode = SocketErrorCode.AccessDenied;
                //    //端口被占用，或没有权限使用
                //    break;
                //case 10014:
                //    //this.ErrorCode = SocketErrorCode.Fault;
                //    //使用了一个无效的指针
                //    break;
                //case 10022:
                //    //this.ErrorCode = SocketErrorCode.InvalidArgument;
                //    //提供了一个无效的参数
                //    break;
                case 10024:
                    //this.ErrorCode = SocketErrorCode.TooManyOpenSockets;
                    //打开的套接字太多
                    //Thread.Sleep(5);
                    handled = true;
                    break;
                case 10035:
                    //this.ErrorCode = SocketErrorCode.WouldBlock;
                    //非阻塞错误
                    socket.Socket.Blocking = true;
                    handled = true;
                    break;
                case 10036:
                    //this.ErrorCode = SocketErrorCode.InProgress;
                    //一个阻塞操作正在进行
                    //Thread.Sleep(5);
                    handled = true;
                    break;
                case 10037:
                    //this.ErrorCode = SocketErrorCode.AlreadyInProgress;
                    //操作正在进行中
                    //Thread.Sleep(5);
                    handled = true;
                    break;
                case 10038:
                    //this.ErrorCode = SocketErrorCode.NotSocket;
                    //无效套接字
                    tell_lose(socket, source);
                    handled = true;
                    break;
                //case 10039:
                //    //this.ErrorCode = SocketErrorCode.DestinationAddressRequired;
                //    //目标地址错误
                //    break;
                //case 10040:
                //    //this.ErrorCode = SocketErrorCode.MessageSize;
                //    //发送/接收buffer不够大，消息过长
                //    break;
                //case 10041:
                //    //this.ErrorCode = SocketErrorCode.MessageSize;
                //    //发送/接收buffer不够大，消息过长
                //    break;
                //case 10042:
                //    //this.ErrorCode = SocketErrorCode.ProtocolOption;
                //    //错误的协议选项
                //    break;
                //case 10043:
                //    //this.ErrorCode = SocketErrorCode.ProtocolNotSupported;
                //    //请求的协议还没有在系统中配置，或者没有它存在的迹象
                //    break;
                //case 10044:
                //    //this.ErrorCode = SocketErrorCode.SocketNotSupported;
                //    //在这个地址家族中不存在对指定的插槽类型的支持
                //    break;
                //case 10045:
                //    //this.ErrorCode = SocketErrorCode.OperationNotSupported;
                //    //不支持该操作
                //    break;
                //case 10046:
                //    //this.ErrorCode = SocketErrorCode.ProtocolFamilyNotSupported;
                //    break;
                //case 10047:
                //    //this.ErrorCode = SocketErrorCode.AddressFamilyNotSupported;
                //    break;
                //case 10048:
                //    //this.ErrorCode = SocketErrorCode.AddressAlreadyInUse;
                //    break;
                case 10050:
                    //this.ErrorCode = SocketErrorCode.NetworkDown;
                    //网关被破坏,路由表出错
                    tell_lose(socket, source);
                    handled = true;
                    break;
                case 10051:
                    //this.ErrorCode = SocketErrorCode.NetworkUnreachable;
                    //向一个无法连接的网络尝试了一个套接字操作
                    tell_lose(socket, source);
                    handled = true;
                    break;
                case 10052:
                    //this.ErrorCode = SocketErrorCode.NetworkReset;
                    tell_lose(socket, source);
                    handled = true;
                    break;
                case 10053:
                    //this.ErrorCode = SocketErrorCode.ConnectionAborted;
                    tell_lose(socket, source);
                    handled = true;
                    break;
                case 10054:
                    //this.ErrorCode = SocketErrorCode.ConnectionReset;
                    tell_lose(socket, source);
                    handled = true;
                    break;
                case 10055:
                    //this.ErrorCode = SocketErrorCode.NoBufferSpaceAvailable;
                    break;
                case 10056:
                    //this.ErrorCode = SocketErrorCode.IsConnected;
                    handled = true;
                    break;
                case 10057:
                    //this.ErrorCode = SocketErrorCode.NotConnected;
                    tell_lose(socket, source);
                    handled = true;
                    break;
                case 10058:
                    //this.ErrorCode = SocketErrorCode.Shutdown;
                    tell_lose(socket, source);
                    handled = true;
                    break;
                case 10060:
                    //this.ErrorCode = SocketErrorCode.TimedOut;
                    //操作超时
                    //if (!server) socket.OnLoseConnected();
                    tell_lose(socket, source);
                    handled = true;
                    break;
                case 10061:
                    //this.ErrorCode = SocketErrorCode.ConnectionRefused;
                    //if (!server) socket.OnLoseConnected();
                    //连接被拒
                    tell_lose(socket, source);
                    handled = true;
                    break;
                //case 10064:
                //    //this.ErrorCode = SocketErrorCode.HostDown;
                //    break;
                //case 10065:
                //    //this.ErrorCode = SocketErrorCode.HostUnreachable;
                //    break;
                case 10067:
                    //this.ErrorCode = SocketErrorCode.ProcessLimit;
                    //Thread.Sleep(10);
                    handled = true;
                    break;
                //case 10091:
                //    //this.ErrorCode = SocketErrorCode.SystemNotReady;
                //    break;
                //case 10092:
                //    //this.ErrorCode = SocketErrorCode.VersionNotSupported;
                //    break;
                //case 10093:
                //    //this.ErrorCode = SocketErrorCode.NotInitialized;
                //    break;
                //case 10101:
                //    //this.ErrorCode = SocketErrorCode.Disconnecting;
                //    break;
                //case 10109:
                //    //this.ErrorCode = SocketErrorCode.TypeNotFound;
                //    break;
                //case 11001:
                //    //this.ErrorCode = SocketErrorCode.HostNotFound;
                //    break;
                case 11002:
                    //this.ErrorCode = SocketErrorCode.TryAgain;
                    handled = true;
                    break;
                //case 11003:
                //    //this.ErrorCode = SocketErrorCode.NoRecovery;
                //    break;
                case 11004:
                    //this.ErrorCode = SocketErrorCode.NoData;
                    //无数据
                    handled = true;
                    break;
                //case -1:
                //default:
                //    //this.ErrorCode = SocketErrorCode.SocketError;
                //    break;
            }

            return handled;
        }

        /// <inheritdoc/>
        public override string Message => this.message;

        /// <summary>
        /// Gets a value indicating whether 已经处理.
        /// </summary>
        public virtual bool Handled => handled;

        /// <summary>
        /// Gets 错误代码.
        /// </summary>
        public virtual SocketErrorCode ErrorCode { get; }

        /// <summary>
        /// Gets 套接字异常.
        /// </summary>
        public virtual SocketException SocketException { get; }

        private static void tell_lose(SocketCore socket, NetPsSocketExceptionSource source)
        {
            //读取中报错，丢失链接
            if (socket != null)
                switch (source)
            {
                case NetPsSocketExceptionSource.Read:
                case NetPsSocketExceptionSource.Connect:
                case NetPsSocketExceptionSource.Write:
                case NetPsSocketExceptionSource.StartWrite:
                case NetPsSocketExceptionSource.Writing:
                case NetPsSocketExceptionSource.EndWrite:
                    socket.Lose();
                    break;
            }
        }
    }
}
