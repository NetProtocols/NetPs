namespace NetPs.Udp.Hole
{
    using NetPs.Socket;
    using NetPs.Socket.Packets;
    using System;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    /// Udp Hole 打洞实现
    /// </summary>
    public class UdpHoleHe : IDisposable, IBindUdpHoleCore
    {
        private bool is_disposed = false;
        private bool is_holed = false;
        public UdpHoleHe()
        {

        }
        public delegate void HoledHandler(UdpHoleCore core);
        public event HoledHandler Holed;
        public virtual bool IsHoled => is_holed;
        public virtual bool IsDisposed => is_disposed;
        public virtual UdpHoleCore Core { get; private set; }
        public virtual string Id { get; private set; }
        public virtual string Key { get; private set; }
        public virtual void BindCore(UdpHoleCore core)
        {
            Core = core;
            core.PacketReceived += Core_PacketReceived;
        }
        public virtual void Hole(string id, string key)
        {
            this.Id = id;
            this.Key = key;
            var packet = new HolePacket(HolePacketOperation.Hole, id, key);
            this.Core.Tx.Transport(packet.GetData());
        }

        private async void Core_PacketReceived(HolePacket packet)
        {
            switch (packet.Operation)
            {
                case HolePacketOperation.Hole:
                    if (packet.IsCallback)
                    {
                        this.is_holed = true;
                        this.Core.UnResolvePacket();
                        this.Core.Connect(packet.Source);
                        Holed?.Invoke(this.Core);
                    }
                    else
                    {
                        await start_hole(packet.Address);
                    }
                    break;
            }
        }
        private async Task start_hole(IPEndPoint ip)
        {
            var tx = Core.GetTx(ip);
            var pkt = new HolePacket(HolePacketOperation.HoleCallback, Id, Key);
            var i = 0;
            while (i < 25)
            {
                tx.Transport(pkt.GetData());
                await Task.Delay(10);
                if (is_holed) return;
            }
        }

        public virtual void Dispose()
        {
            lock (this)
            {
                if(is_disposed) return;
                is_disposed = true;
            }
        }
    }
}
