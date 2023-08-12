namespace NetPs.Socket
{
    using System;
    using System.Net.Sockets;

    /// <summary>
    /// 异常.
    /// </summary>
    public class NetPsSocketException : Exception
    {
        private bool handled = false;

        private string message;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetPsSocketException"/> class.
        /// </summary>
        /// <param name="errorCode">错误代码.</param>
        /// <param name="msg">消息.</param>
        /// <param name="socketException">套接字异常.</param>
        public NetPsSocketException(SocketErrorCode errorCode, string msg, SocketException socketException = null)
        {
            this.ErrorCode = errorCode;
            this.SocketException = socketException;
            this.message = msg;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetPsSocketException"/> class.
        /// </summary>
        /// <param name="e">SocketException.</param>
        /// <param name="socket">SocketCore.</param>
        public NetPsSocketException(SocketException e, SocketCore socket)
        {
            this.message = e.Message;
            this.SocketException = e;
            switch ((int)e.ErrorCode)
            {
                case 0:
                    this.ErrorCode = SocketErrorCode.Success;
                    this.handled = true;
                    break;
                case 995:
                    this.ErrorCode = SocketErrorCode.OperationAborted;
                    break;
                case 997:
                    this.ErrorCode = SocketErrorCode.IOPending;
                    break;
                case 10004:
                    this.ErrorCode = SocketErrorCode.Interrupted;
                    break;
                case 10013:
                    this.ErrorCode = SocketErrorCode.AccessDenied;
                    break;
                case 10014:
                    this.ErrorCode = SocketErrorCode.Fault;
                    break;
                case 10022:
                    this.ErrorCode = SocketErrorCode.InvalidArgument;
                    break;
                case 10024:
                    this.ErrorCode = SocketErrorCode.TooManyOpenSockets;
                    break;
                case 10035:
                    this.ErrorCode = SocketErrorCode.WouldBlock;
                    break;
                case 10036:
                    this.ErrorCode = SocketErrorCode.InProgress;
                    break;
                case 10037:
                    this.ErrorCode = SocketErrorCode.AlreadyInProgress;
                    break;
                case 10038:
                    this.ErrorCode = SocketErrorCode.NotSocket;
                    break;
                case 10039:
                    this.ErrorCode = SocketErrorCode.DestinationAddressRequired;
                    break;
                case 10040:
                    this.ErrorCode = SocketErrorCode.MessageSize;
                    break;
                case 10041:
                    this.ErrorCode = SocketErrorCode.MessageSize;
                    break;
                case 10042:
                    this.ErrorCode = SocketErrorCode.ProtocolOption;
                    break;
                case 10043:
                    this.ErrorCode = SocketErrorCode.ProtocolNotSupported;
                    break;
                case 10044:
                    this.ErrorCode = SocketErrorCode.SocketNotSupported;
                    break;
                case 10045:
                    this.ErrorCode = SocketErrorCode.OperationNotSupported;
                    break;
                case 10046:
                    this.ErrorCode = SocketErrorCode.ProtocolFamilyNotSupported;
                    break;
                case 10047:
                    this.ErrorCode = SocketErrorCode.AddressFamilyNotSupported;
                    break;
                case 10048:
                    this.ErrorCode = SocketErrorCode.AddressAlreadyInUse;
                    break;
                case 10050:
                    this.ErrorCode = SocketErrorCode.NetworkDown;
                    break;
                case 10051:
                    this.ErrorCode = SocketErrorCode.NetworkUnreachable;
                    break;
                case 10052:
                    this.ErrorCode = SocketErrorCode.NetworkReset;
                    break;
                case 10053:
                    this.ErrorCode = SocketErrorCode.ConnectionAborted;
                    socket.OnLoseConnected();
                    this.handled = true;
                    break;
                case 10054:
                    this.ErrorCode = SocketErrorCode.ConnectionReset;
                    socket.OnLoseConnected();
                    this.handled = true;
                    break;
                case 10055:
                    this.ErrorCode = SocketErrorCode.NoBufferSpaceAvailable;
                    break;
                case 10056:
                    this.ErrorCode = SocketErrorCode.IsConnected;
                    this.handled = true;
                    break;
                case 10057:
                    this.ErrorCode = SocketErrorCode.NotConnected;
                    socket.OnLoseConnected();
                    break;
                case 10058:
                    this.ErrorCode = SocketErrorCode.Shutdown;
                    socket.OnLoseConnected();
                    break;
                case 10060:
                    this.ErrorCode = SocketErrorCode.TimedOut;
                    socket.OnLoseConnected();
                    break;
                case 10061:
                    this.ErrorCode = SocketErrorCode.ConnectionRefused;
                    socket.OnLoseConnected();
                    this.handled = true;
                    break;
                case 10064:
                    this.ErrorCode = SocketErrorCode.HostDown;
                    break;
                case 10065:
                    this.ErrorCode = SocketErrorCode.HostUnreachable;
                    break;
                case 10067:
                    this.ErrorCode = SocketErrorCode.ProcessLimit;
                    break;
                case 10091:
                    this.ErrorCode = SocketErrorCode.SystemNotReady;
                    break;
                case 10092:
                    this.ErrorCode = SocketErrorCode.VersionNotSupported;
                    break;
                case 10093:
                    this.ErrorCode = SocketErrorCode.NotInitialized;
                    break;
                case 10101:
                    this.ErrorCode = SocketErrorCode.Disconnecting;
                    break;
                case 10109:
                    this.ErrorCode = SocketErrorCode.TypeNotFound;
                    break;
                case 11001:
                    this.ErrorCode = SocketErrorCode.HostNotFound;
                    break;
                case 11002:
                    this.ErrorCode = SocketErrorCode.TryAgain;
                    break;
                case 11003:
                    this.ErrorCode = SocketErrorCode.NoRecovery;
                    break;
                case 11004:
                    this.ErrorCode = SocketErrorCode.NoData;
                    break;
                case -1:
                default:
                    this.ErrorCode = SocketErrorCode.SocketError;
                    break;
            }
        }

        /// <inheritdoc/>
        public override string Message => this.message;

        /// <summary>
        /// Gets a value indicating whether 已经处理.
        /// </summary>
        public virtual bool Handled => this.handled;

        /// <summary>
        /// Gets 错误代码.
        /// </summary>
        public virtual SocketErrorCode ErrorCode { get; }

        /// <summary>
        /// Gets 套接字异常.
        /// </summary>
        public virtual SocketException SocketException { get; }
    }
}
