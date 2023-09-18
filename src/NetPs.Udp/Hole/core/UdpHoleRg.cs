namespace NetPs.Udp.Hole
{
    using NetPs.Socket;
    using NetPs.Socket.Packets;
    using System;
    using System.Threading;

    /// <summary>
    /// Udp Hole 注册实现
    /// </summary>
    public class UdpHoleRg : IDisposable, IBindUdpHoleCore
    {
        private bool is_disposed = false;
        private bool is_registered = false;
        private CancellationToken CancellationToken { get; set; }
        public UdpHoleRg()
        {
            this.CancellationToken = new CancellationToken();
        }

        public virtual bool IsDisposed => is_disposed;
        public virtual bool IsRegistered => is_registered;
        public virtual UdpHoleCore Core { get; private set; }

        public virtual void BindCore(UdpHoleCore core)
        {
            Core = core;
            core.PacketReceived += Core_PacketReceived;
        }
        public virtual void Register(string id, string key)
        {
            var packet = new HolePacket(HolePacketOperation.Register, id, key);
            this.is_registered = false;
            this.Core.Tx.Transport(packet.GetData());
        }
        private void Core_PacketReceived(HolePacket packet)
        {
            switch(packet.Operation)
            {
                case HolePacketOperation.Register:
                    if (packet.IsCallback)
                    {
                        this.is_registered = true;
                    }
                    break;
            }
        }
        public virtual void Dispose()
        {
            lock (this)
            {
                if (is_disposed) return;
                is_disposed = true;
            }
            if (Core != null)
            {
                Core.PacketReceived -= Core_PacketReceived;
            }
        }
    }
}
