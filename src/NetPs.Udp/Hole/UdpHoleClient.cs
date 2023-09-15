namespace NetPs.Udp.Hole
{
    using NetPs.Socket;
    using NetPs.Socket.Hole;
    using NetPs.Socket.Packets;
    using System;
    public class UdpHoleClient : UdpHoleCore, IDisposable
    {
        private bool is_disposed = false;
        public UdpHoleClient() : base()
        {
            He = new UdpHoleHe();
            Rg = new UdpHoleRg();
            Fz = new UdpHoleFz();
            He.BindCore(this);
            Rg.BindCore(this);
            Fz.BindCore(this);
        }
        public virtual UdpHoleHe He { get; private set; }
        public virtual UdpHoleRg Rg { get; private set; }
        public virtual UdpHoleFz Fz { get; private set; }
        public override void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
        }
    }
}
