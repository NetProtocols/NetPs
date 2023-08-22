namespace NetPs.Tcp
{
    using System;

    public interface ITcpAccept
    {
        bool TcpAccept(TcpServer server, TcpClient client);
    }
}
