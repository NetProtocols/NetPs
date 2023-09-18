namespace NetPs.Udp.Hole
{
    using NetPs.Socket;
    using NetPs.Socket.Packets;
    using System;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    /// <summary>
    /// Udp Host 辅助实现
    /// </summary>
    /// <remarks>
    /// 主要目的：预测端口、找寻真实端口，Hole可行性检测。
    /// 01. 服务端使用时，不使用端口复用。
    /// 02. 客户端使用时，使用端口复用。
    /// 检测方法，与上次端口相同则为True，不同则进行二次验证，相同则为True，不同时，当有规律可循推算，反之不可达。
    /// </remarks>
    public class UdpHoleFz : IDisposable, IBindUdpHoleCore
    {
        private bool is_disposed = false;
        private UdpHoleCore host { get; set; }
        private Dictionary<string, bool> fz_callback { get; set; }
        public UdpHoleFz()
        {
            fz_callback = new Dictionary<string, bool>();
        }
        public delegate void FzCallback();
        public virtual bool IsDisposed => is_disposed;

        public virtual UdpHoleCore Core { get; private set; }
        public virtual void BindCore(UdpHoleCore core)
        {
            this.Core = core;
            this.Core.PacketReceived += Core_PacketReceived;
        }
        private async void Core_PacketReceived(HolePacket packet)
        {
            switch (packet.Operation)
            {
                case HolePacketOperation.Fuzhu:
                    if (packet.IsCallback)
                    {
                        fz_callback[packet.FuzhuTag] = true;
                    }
                    else
                    {
                        await start_fuzhu(packet.FuzhuTag, packet.FuzhuPorts);
                    }
                    break;
            }
        }

        private async Task start_fuzhu(string tag, int[] ports)
        {
            foreach (var port in ports)
            {
                var tx = Core.GetTx(this.Core.ServerAddress.IP, port);
                var i = 0;
                var pkt = new HolePacket();
                pkt.Fuzhu(tag);
                while (i++ < 25)
                {
                    tx.Transport(pkt.GetData());
                    await Task.Delay(10);
                    if (has_fz(tag)) return;
                }
            }
        }

        private bool has_fz(string tag) => fz_callback.ContainsKey(tag) && fz_callback[tag];
        public virtual void Dispose()
        {
            lock (this)
            {
                if (is_disposed) return;
                is_disposed = true;
            }
        }
    }
}
