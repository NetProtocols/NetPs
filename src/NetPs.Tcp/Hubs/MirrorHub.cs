namespace NetPs.Tcp
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public class MirrorHub : HubBase, IHub
    {
        private bool is_disposed = false;
        private bool is_init = false;
        private TcpRepeaterClient tcp { get; set; }
        private readonly long id = GetId();
        private TcpRepeaterClient mirror { get; set; }

        public virtual string Mirror_Address { get; protected set; }
        public MirrorHub(TcpClient tcp, string mirror_addr, int limit)
        {
            if (!tcp.Actived) return;
            this.Mirror_Address = mirror_addr;
            this.mirror = new TcpRepeaterClient(tcp.Tx);
            this.tcp = new TcpRepeaterClient(tcp, this.mirror.Tx);
            this.mirror.Limit(limit);
            this.tcp.Limit(limit);
            mirror.SocketClosed += Mirror_SocketClosed;
            this.tcp.SocketClosed += Tcp_SocketClosed; ;
            mirror.Connected += Mirror_Connected;
            this.is_init = true;
        }

        private void Tcp_SocketClosed(object sender, EventArgs e)
        {
            this.tcp.SocketClosed -= Tcp_SocketClosed;
            if (!is_disposed) this.Dispose();
        }

        private void Mirror_SocketClosed(object sender, EventArgs e)
        {
            mirror.SocketClosed -= Mirror_SocketClosed;
            try
            {
                //关闭告知
                if (this.tcp != null)
                {
                    this.tcp.FIN();
                }
            }
            catch
            {
                Debug.Assert(false);
            }
        }

        private void Mirror_Connected(object source)
        {
            mirror.Rx.StartReceive();
            this.tcp.Rx.StartReceive();
        }

        public virtual long ID => this.id;

        public virtual void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            if (this.mirror != null)
            {
                mirror.Connected -= Mirror_Connected;
            }
            this.Close();
        }

        public async void Start()
        {
            try
            {
                if (!is_init) return;
                var ok = await mirror.ConnectAsync(this.Mirror_Address);
                if (!ok)
                {
                    //this.Close();
                }
                return;
            }
            catch when (this.is_disposed) { Debug.Assert(false); }
            catch (Exception e)
            {
                Debug.Assert(false);
                //this.Close();
                Hub.ThrowException(e);
            }
        }

        protected override void OnClosed()
        {
            if (this.mirror != null && this.mirror.Actived)
            {
                this.mirror.FIN();
            }
        }
    }
}
