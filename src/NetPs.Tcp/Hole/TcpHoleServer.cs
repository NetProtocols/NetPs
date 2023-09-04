namespace NetPs.Tcp.Hole
{
    using System;

    public class TcpHoleServer : ITcpServerEvents
    {
        private TcpServer server { get; set; }
        public TcpHoleServer()
        {
            server = new TcpServer();
            server.BindEvents(this);
        }

        public virtual void Run(string uri)
        {
            server.Listen(uri);
        }

        public void OnAccepted(ITcpServer tcpServer, ITcpClient tcpClient)
        {
        }

        public void OnClosed(ITcpServer tcpServer)
        {
        }

        public void OnConfiguration(ITcpServer tcpServer)
        {
        }

        public void OnConfiguration(TcpCore core)
        {
        }

        public void OnListened(ITcpServer tcpServer)
        {
        }

        public void OnSocketLosed(ITcpServer tcpServer, ITcpClient tcpClient)
        {
        }
    }
}
