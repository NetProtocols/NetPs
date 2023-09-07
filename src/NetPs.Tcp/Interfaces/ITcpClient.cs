namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    public interface ITcpClient
    {
        /// <summary>
        /// 绑定事件处理
        /// </summary>
        void BindEvents(ITcpClientEvents events);
        void BindRxEvents(IRxEvents events);
        void BindTxEvents(ITxEvents events);

        void StartReceive();
        void Transport(byte[] data);
        void Transport(byte[] data, int offset, int length);
    }
}
