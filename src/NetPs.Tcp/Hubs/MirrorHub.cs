namespace NetPs.Tcp
{
    using NetPs.Tcp.Interfaces;
    using System;
    using System.Reactive.Linq;
    using System.Threading;

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
            this.mirror.Disposables.Add(mirror.ConnectedObservable.Subscribe(_ =>
            {
                mirror.Rx.StartReceive();
                this.tcp.Rx.StartReceive();
            }));
        }

        private void Mirror_DisConnected(System.Net.IPEndPoint iPEndPoint)
        {
            lock (this)
            {
                if (!is_disposed) this.Dispose();
            }
        }

        public long ID => this.id;

        public void Dispose()
        {
            this.is_disposed = true;
            this.Close();
            if (this.mirror != null)
            {
                mirror.DisConnected -= Mirror_DisConnected;
                this.mirror.Dispose();
                this.mirror = null;
            }
            if (this.tcp != null)
            {
                this.tcp.DisConnected -= Mirror_DisConnected;
                this.tcp.Dispose();
                this.tcp = null;
            }
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
                Hub.ThrowException(e);
            }
        }
        private void Hub_Closed(object sender, EventArgs e)
        {
            this.Closed -= Hub_Closed;
            this.tcp.Shutdown();
        }
    }
}
