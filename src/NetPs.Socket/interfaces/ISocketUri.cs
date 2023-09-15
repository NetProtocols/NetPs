namespace NetPs.Socket
{
    using System;
    using System.Net;

    public interface ISocketUri
    {
        string Scheme { get; }
        IPAddress IP { get; }
        string Host { get; }
        int Port { get; }

        bool Equal(IPEndPoint iPEndPoint);
        bool Equal(ISocketUri host);
        string ToString();
    }
}
