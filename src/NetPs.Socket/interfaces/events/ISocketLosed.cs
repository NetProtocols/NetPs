namespace NetPs.Socket
{
    using System;

    /// <summary>
    /// 连接丢失事件
    /// </summary>
    public interface ISocketLosed
    {
        void OnSocketLosed(object? socket);
    }
}
