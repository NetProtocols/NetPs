namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;

    public interface ITcpTx : IDataTransport
    {
        void BindEvents(ITcpTxEvents events);
    }
}
