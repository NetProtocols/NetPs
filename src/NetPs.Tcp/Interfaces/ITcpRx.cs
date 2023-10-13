namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    public interface ITcpRx : IRx, IBindTcpCore
    {
        int BufferSize { get; }
        IObservable<byte[]> ReceivedObservable { get; }
        void WhenReceived(ITcpReceive tcp_receive);
    }
}
