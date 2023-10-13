namespace NetPs.Socket
{
    using System;
    using System.Net;

    public interface ITx : IDataTransport
    {
        IPEndPoint RemoteAddress { get; }
        int TransportBufferSize { get; }
        void BindEvents(ITxEvents events);
        void SetTransportBufferSize(int size);
    }
}
