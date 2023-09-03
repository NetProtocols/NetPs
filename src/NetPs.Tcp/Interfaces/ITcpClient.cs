namespace NetPs.Tcp
{
    using System;
    public interface ITcpClient
    {
        /// <summary>
        /// 绑定事件处理
        /// </summary>
        void BindEvents(ITcpClientEvents events);
    }
}
