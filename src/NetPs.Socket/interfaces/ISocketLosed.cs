namespace NetPs.Socket
{
    using System;
    public interface ISocketLosed
    {
        void OnSocketLosed(object? socket);
    }
}
