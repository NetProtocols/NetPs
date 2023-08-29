namespace NetPs.Socket
{
    using System;
    using System.Net;

    public interface IReceivedFrom
    {
        void Received(byte[] data, EndPoint endPoint);
    }
}
