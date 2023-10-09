namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;

    public interface ITcpTx : ITx, IBindTcpCore
    {
        IObservable<TcpTx> TransportedObservable { get; }
    }
}
