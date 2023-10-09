namespace NetPs.Socket
{
    using System;
    using System.Net;

    public interface IRx : IDisposable
    {
        byte[] Buffer { get; }
        int ReceivedSize { get; }
        bool Running { get; }
        IPEndPoint RemoteAddress { get; }
        void BindEvents(IRxEvents events);
    }
}
