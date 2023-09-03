namespace NetPs.Tcp
{
    using System;
    public interface ITcpServer
    {
        void BindEvents(ITcpServerEvents events);
    }
}
