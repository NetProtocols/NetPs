namespace NetPs.Tcp
{
    using System;

    public interface ITcpReceive
    {
        void TcpReceive(byte[] data, ITcpClient tcp);
    }
}
