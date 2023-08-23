namespace NetPs.Tcp
{
    using System;

    public class MirrorHub : HubBase, IHub
    {
        private TcpClient tcp;
        private readonly long id = GetId();
        private TcpClient mirror;

        public virtual string Mirror_Address { get; protected set; }
        public MirrorHub(TcpClient tcp, string mirror_addr)
        {
            this.Mirror_Address = mirror_addr;
            this.tcp = tcp;
            this.mirror = new TcpClient();
            this.tcp.Disposables.Add(this.tcp.ReceivedObservable.Subscribe(mirror.Tx.Transport));
            this.mirror.Disposables.Add(mirror.ReceivedObservable.Subscribe(tcp.Tx.Transport));
            this.mirror.Disposables.Add(mirror.LoseConnectedObservable.Subscribe(_ =>
            {
                this.Dispose();
            }));
            this.mirror.Disposables.Add(mirror.ConnectedObservable.Subscribe(_ =>
            {
                mirror.Rx.StartReceive();
                this.tcp.Rx.StartReceive();
            }));
        }
        public long ID => this.id;

        public void Dispose()
        {
            this.Close();
            this.mirror?.Dispose();
        }

        public void Start()
        {
            try
            {
                this.Closed += Hub_Closed;
                mirror.Connect(this.Mirror_Address);
            }
            catch (Exception e)
            {
                var ok = ThrowException(e);
                if (!ok)
                {
                    throw e;
                }
            }
        }
        private void Hub_Closed(object sender, EventArgs e)
        {
            this.Closed -= Hub_Closed;
            this.tcp.Shutdown();
        }
    }
}
