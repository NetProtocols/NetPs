namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Net;

    public interface IUdpHost : IClient, IDisposable
    {
        IUdpRx Rx { get; }
        IObservable<UdpData> ReceivedObservable { get; }
        void StartReceive();
        void StartReceive(Action<UdpData> action);
        IUdpTx GetTx(IPEndPoint address);
        IUdpTx GetTx(IPAddress ip, int port);
        IUdpTx GetTx(string address);
    }
}
