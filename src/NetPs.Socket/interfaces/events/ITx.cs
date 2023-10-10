namespace NetPs.Socket
{
    using System;
    public interface ITx : IDataTransport
    {
        int TransportBufferSize { get; }
        void BindEvents(ITxEvents events);
        void SetTransportBufferSize(int size);
    }
}
