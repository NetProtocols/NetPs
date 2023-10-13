namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    public interface ITcpClient : IClient, IDisposable
    {

        /// <summary>
        /// 接收.
        /// </summary>
        ITcpRx Rx { get; }

        /// <summary>
        /// 发送.
        /// </summary>
        ITcpTx Tx { get; }
        IHub Hub { get; }
        /// <summary>
        /// 绑定事件处理
        /// </summary>
        void BindEvents(ITcpClientEvents events);
        void BindRxEvents(IRxEvents events);
        void BindTxEvents(ITxEvents events);

        void StartReceive();
        void StartReceive(ITcpReceive receive);
        void Transport(byte[] data);
        void Transport(byte[] data, int offset, int length);
    }
}
