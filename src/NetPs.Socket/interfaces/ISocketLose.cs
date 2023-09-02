namespace NetPs.Socket
{
    using System;
    public interface ISocketLose
    {
        void OnSocketLosed(object? socket);
    }
}
