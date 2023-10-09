namespace NetPs.Tcp
{
    using System;
    using System.Diagnostics;

    public class MirrorHub : HubBase, IHub
    {
        private bool is_disposed = false;
        private bool is_init = false;
        public virtual string Mirror_Address { get; protected set; }
        private TcpRepeaterClient tcp { get; set; }
        private TcpRepeaterClient mirror { get; set; }
        public MirrorHub(ITcpClient tcp, string mirror_addr, int limit)
        {
            if (!tcp.Actived) return;
            this.Mirror_Address = mirror_addr;
            this.mirror = new TcpRepeaterClient(tcp.Tx);
            this.tcp = new TcpRepeaterClient(tcp, this.mirror.Tx);
            this.mirror.Limit(limit);
            this.tcp.Limit(limit);
            mirror.SocketClosed += Mirror_SocketClosed;
            this.tcp.SocketClosed += Tcp_SocketClosed; ;
            this.is_init = true;
        }

        private void Tcp_SocketClosed(object sender, EventArgs e)
        {
            this.tcp.SocketClosed -= Tcp_SocketClosed;
            if (this.mirror.IsClosed)
            {
                this.Dispose();
            }
            else
            {
                this.mirror.FIN();
            }
        }

        private void Mirror_SocketClosed(object sender, EventArgs e)
        {
            mirror.SocketClosed -= Mirror_SocketClosed;
            //关闭告知
            if (this.tcp.IsClosed)
            {
                this.Dispose();
            }
            else
            {
                this.tcp.FIN();
            }
        }


        public virtual void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            } 
            this.Close();
        }

        public void Start()
        {
            try
            {
                if (!is_init) return;
                var ok = mirror.Connect(this.Mirror_Address);
                if (!ok)
                {
                    this.mirror.Lose();
                    return;
                }
                if (this.tcp.Actived)
                {
                    mirror.Rx.StartReceive();
                    this.tcp.Rx.StartReceive();
                }
                else
                {
                    this.mirror.FIN();
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
        }
    }
}
