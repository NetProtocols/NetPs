namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    using System.Net;

    /// <summary>
    /// 接收流.
    /// </summary>
    /// <param name="stream">流.</param>
    public delegate void ReveicedStreamHandler(UdpData data);
    public delegate bool ReveicedNoBufferHandler(byte[] buffer, int length, IPEndPoint address);
    public interface IUdpRx :IRx, IBindUdpCore
    {
        event ReveicedStreamHandler Received;
        event ReveicedNoBufferHandler NoBufferReceived;
        IObservable<UdpData> ReceivedObservable { get; }
        /// <summary>
        /// 设置读取数据的远程地址
        /// </summary>
        void SetRemoteAddress(IPEndPoint address);
        void UseRx(IUdpRx rx);
    }
}
