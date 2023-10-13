namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    public sealed class TcpMirrorHub : MirrorHub<TcpRepeaterClient>
    {
        public TcpMirrorHub(IClient client, string mirror_addr, int limit) : base(client, mirror_addr, limit) { }
    }

    public class TcpRepeaterClient : TcpClientFactory<TcpTx, TcpRxRepeater>, IRepeaterClient, IDisposable
    {
        public TcpRepeaterClient() : base() { }
        public virtual void Limit(int limit)
        {
            if (this.Rx is ISpeedLimit limiter)
            {
                limiter.SetLimit(limit);
            }
        }
        public void UseTx(IClient client)
        {
            if (this.Rx is TcpRxRepeater reapter_rx)
            {
                reapter_rx.BindTransport(client.GetTx());
            }
        }
        public void UseRx(IClient client)
        {
            this.PutSocket(client.Socket);
        }
        public void StartClient(string addr)
        {
            this.Connect(addr);
        }
        public void StopClient()
        {
            this.FIN();
        }
    }
}
