namespace NetPs.Tcp
{
    using System;
    public interface ITcpRx
    {
        byte[] Buffer { get; }
        int ReceivedSize { get; }
        int BufferSize { get; }
        bool Running { get; }
        void BindEvents(ITcpRxEvents events);
    }
}
