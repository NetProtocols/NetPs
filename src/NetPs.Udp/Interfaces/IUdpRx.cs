namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;

    /// <summary>
    /// 接收流.
    /// </summary>
    /// <param name="stream">流.</param>
    public delegate void ReveicedStreamHandler(UdpData data);

    public interface IUdpRx :IRx, IBindUdpCore
    {
        event ReveicedStreamHandler Received;
        IObservable<UdpData> ReceicedObservable { get; }
        void StartReveice();
    }
}
