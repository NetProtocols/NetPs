namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Net;

    public interface IUdpHost : ISocket, IDisposable
    {
        IUdpRx Rx { get; }
        IObservable<UdpData> ReceicedObservable { get; }
        void StartReveice();
        void StartReveice(Action<UdpData> action);
        IUdpTx GetTx(IPEndPoint address);
        IUdpTx GetTx(IPAddress ip, int port);
        IUdpTx GetTx(string address);
    }
}
