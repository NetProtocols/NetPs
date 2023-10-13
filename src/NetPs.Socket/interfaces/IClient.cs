namespace NetPs.Socket
{
    using System;
    public interface IClient : ISocket
    {
        ITx GetTx();
        IRx GetRx();
    }
}
