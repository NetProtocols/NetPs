namespace NetPs.Tcp
{
    using System;
    public interface ITcpServerEvents : ITcpConfig
    {
        void OnConfiguration(ITcpServer tcpServer);
        void OnListened(ITcpServer tcpServer);
        void OnAccepted(ITcpServer tcpServer, ITcpClient tcpClient);
        void OnSocketLosed(ITcpServer tcpServer, ITcpClient tcpClient);
        void OnClosed(ITcpServer tcpServer);
    }
}
