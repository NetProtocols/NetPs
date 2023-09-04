namespace NetPs.Tcp
{
    using System;

    public class MirrorHub : HubBase, IHub
    {
        private bool is_disposed = false;
        private TcpRepeaterClient tcp { get; set; }
        private readonly long id = GetId();
        private TcpRepeaterClient mirror { get; set; }

        public virtual string Mirror_Address { get; protected set; }
        public MirrorHub(TcpClient tcp, string mirror_addr, int limit)
        {
            if (tcp.IsDisposed || !tcp.Actived) return;
            this.Mirror_Address = mirror_addr;
            this.mirror = new TcpRepeaterClient(tcp.Tx);
            this.tcp = new TcpRepeaterClient(tcp, this.mirror.Tx);
            this.mirror.Limit(limit);
            this.tcp.Limit(limit);
            mirror.DisConnected += Mirror_DisConnected;
            this.tcp.DisConnected += Mirror_DisConnected;
            mirror.Connected += Mirror_Connected;
        }

        private void Mirror_Connected(object source)
        {
            mirror.Rx.StartReceive();
            this.tcp.Rx.StartReceive();
        }

        private void Mirror_DisConnected(object source)
        {
            if (!is_disposed) this.Dispose();
        }

        public long ID => this.id;

        public void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            this.Close();
            if (this.mirror != null)
            {
                mirror.Connected -= Mirror_Connected;
                mirror.DisConnected -= Mirror_DisConnected;
                this.mirror.Lose();
                this.mirror = null;
            }
            if (this.tcp != null)
            {
                this.tcp.DisConnected -= Mirror_DisConnected;
                this.tcp.Lose();
                this.tcp = null;
            }
        }

        public async void Start()
        {
            try
            {
                var ok = await mirror.ConnectAsync(this.Mirror_Address);
                if (!ok)
                {
                    this.Close();
                }
                return;
            }
            catch (Exception e)
            {
                //mirror.Lose();
                this.Close();
                Hub.ThrowException(e);
            }
        }

        protected override void OnClosed()
        {
            this.tcp.FIN();
        }
    }
}
