namespace NetPs.Socket
{
    using System;
    public interface ITx : IDataTransport
    {
        void BindEvents(ITxEvents events);
    }
}
