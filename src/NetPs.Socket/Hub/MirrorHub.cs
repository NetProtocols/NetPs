namespace NetPs.Socket
{
    using System;
    using System.Diagnostics;

    public class MirrorHub<TReapter> : HubBase, IHub
        where TReapter : IRepeaterClient, new()
    {
        private bool is_disposed = false;
        private bool is_init = false;
        public virtual string Mirror_Address { get; protected set; }
        private IRepeaterClient client { get; set; }
        private IRepeaterClient mirror { get; set; }
        public MirrorHub(IClient client, string mirror_addr, int limit)
        {
            if (!client.Actived) return;
            this.Mirror_Address = mirror_addr;
            this.mirror = new TReapter();
            this.mirror.UseTx(client);
            this.client = new TReapter();
            this.client.UseRx(client);
             
            this.mirror.Limit(limit);
            this.client.Limit(limit);
            this.is_init = true;
        }
        private void OnSocketClosed(object sender, EventArgs e)
        {
            mirror.SocketClosed -= OnSocketClosed;
            client.SocketClosed -= OnSocketClosed;
            cmd_stop();
            this.Dispose();
        }
        internal void cmd_stop()
        {
            if (!this.mirror.IsClosed)
            {
                this.mirror.StopClient();
            }
            if (!this.client.IsClosed)
            {
                this.client.StopClient();
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
            if (!is_init) return;
            try
            {
                this.mirror.StartClient(this.Mirror_Address);
                if (this.mirror.Actived && this.client.Actived)
                {
                    //监听断开事件
                    mirror.SocketClosed += OnSocketClosed;
                    this.client.SocketClosed += OnSocketClosed;

                    this.client.UseTx(this.mirror);
                    this.mirror.GetRx().StartReceive();
                    this.client.GetRx().StartReceive();
                }
                else
                {
                    cmd_stop();
                }
                return;
            }
            catch when (this.is_disposed) { Debug.Assert(false); }
            catch (Exception e)
            {
                Debug.Assert(false);
                Hub.ThrowException(e);
            }
            cmd_stop();
        }

        protected override void OnClosed()
        {
        }
    }
}
