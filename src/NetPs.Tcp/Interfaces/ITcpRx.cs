namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    public interface ITcpRx : IRx
    {
        int BufferSize { get; }
        void BindEvents(ITcpRxEvents events);
    }
}
