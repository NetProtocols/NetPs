namespace NetPs.Tcp
{
    using System;

    public interface ITcpReceive
    {
        void TcpReceive(byte[] data, ITcpClient tcp);
    }

    public static class ITcpReceiveExtra
    {
        public static void TcpReceive(this ITcpReceive tcpReceive, byte[] data, TcpCore core)
        {
            if (core is ITcpClient tcpClient)
            {
                tcpReceive.TcpReceive(data, tcpClient);
            }
        }
    }
}
