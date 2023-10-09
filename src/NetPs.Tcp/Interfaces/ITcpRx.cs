namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    public interface ITcpRx : IRx, IBindTcpCore
    {
        int BufferSize { get; }
        IObservable<byte[]> ReceivedObservable { get; }
        void StartReceive();
        void WhenReceived(ITcpReceive tcp_receive);
    }
}
