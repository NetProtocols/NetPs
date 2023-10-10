namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Net;

    public interface IUdpTx : ITx, IBindUdpCore, IDisposables
    {
        IPEndPoint RemoteIP { get; }
        IObservable<IUdpTx> TransportedObservable {get;}
        void SetRemote(IPEndPoint endPoint);
    }
}
