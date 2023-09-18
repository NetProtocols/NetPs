namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;

    public interface ITcpTx : ITx
    {
        void BindEvents(ITcpTxEvents events);
    }
}
