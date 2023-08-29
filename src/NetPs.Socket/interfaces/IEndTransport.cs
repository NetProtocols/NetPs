namespace NetPs.Socket
{
    using System;
    public interface IEndTransport
    {
        void WhenTransportEnd(IDataTransport transport);
    }
}
