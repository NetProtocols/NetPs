namespace NetPs.Socket
{
    using System;
    using System.Net.Sockets;

    public abstract class SocketFunc
    {

        /// <summary>
        /// Gets or sets tcp client.
        /// </summary>
        public virtual Socket Socket { get; protected set; }

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
    }
}
