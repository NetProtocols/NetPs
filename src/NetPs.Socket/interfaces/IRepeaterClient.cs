namespace NetPs.Socket
{
    using System;
    public interface IRepeaterClient : IClient
    {
        event EventHandler SocketClosed;
        void Limit(int limit);
        void UseTx(IClient client);
        void UseRx(IClient client);
        void StartClient(string addr);
        void StopClient();
    }
}
