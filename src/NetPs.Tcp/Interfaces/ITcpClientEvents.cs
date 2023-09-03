namespace NetPs.Tcp
{
    using System;

    public interface ITcpClientEvents : ITcpConfig
    {
        void OnConfiguration(ITcpClient client);
        void OnConnected(ITcpClient client);
        void OnDisconnected(ITcpClient client);
        void OnClosed(ITcpClient client);
        void OnLosed(ITcpClient client);
    }
}
