namespace NetPs.Udp
{
    using NetPs.Socket;
    using System;
    public sealed class UdpMirrorHub : MirrorHub<UdpRepeaterHost>
    {
        public UdpMirrorHub(IClient client, string mirror_addr, int limit) : base(client, mirror_addr, limit) { }
    }

    public class UdpRepeaterHost : UdpHostFactory<UdpTx, UdpRxRepeater>, IRepeaterClient, IDisposable
    {
        public UdpRepeaterHost() : base() { }
        public void Limit(int limit)
        {
            if (this.Rx is ISpeedLimit limiter)
            {
                limiter.SetLimit(limit);
            }
        }
        public void StartClient(string addr)
        {
            this.Bind("0.0.0.0:0");
            this.Connect(addr);
        }
        public void StopClient()
        {
            this.Lose();
        }
        public void UseRx(IClient client)
        {
            this.PutSocket(client.Socket);
            if (client.GetRx() is IUdpRx udp_rx)
            {
                this.Rx.UseRx(udp_rx);
                this.Connect(client.RemoteIPEndPoint);
            }
        }
        public void UseTx(IClient client)
        {
            if (this.Rx is UdpRxRepeater reapter_rx)
            {
                var tx = client.GetTx();
                reapter_rx.BindTransport(tx);
            }
        }
    }
}
