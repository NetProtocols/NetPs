namespace NetPs.Udp.Hole
{
    using NetPs.Socket;
    using NetPs.Socket.Packets;
    using System;
    using System.Net;

    /// <summary>
    /// Udp Hole 核心
    /// </summary>
    public class UdpHoleCore : IDisposable
    {
        private bool is_server = false;
        private bool is_disposed = false;
        private bool is_resolving = false;
        private bool is_connected = false;
        private UdpHost host { get; set; }
        public UdpHoleCore()
        {
        }

        public delegate void PacketReceivedHandle(HolePacket packet);
        public event PacketReceivedHandle PacketReceived;
        public event ReveicedStreamHandler Received;
        public virtual bool IsServer => is_server;
        public virtual bool IsDisposed => is_disposed;
        public virtual bool IsResolving => is_resolving;
        public virtual bool IsConnected => is_connected;
        public virtual ISocketUri Address => this.host.Address;
        public virtual ISocketUri ServerAddress { get; private set; }
        public virtual ITx Tx { get; private set; }
        public virtual void Run(string address)
        {
            this.host = new UdpHost(address);
            this.ResolvePacket();
            this.host.Rx.StartReveice();
            this.OnRun();
        }
        public virtual void Connect(string server)
        {
            this.ServerAddress = new InsideSocketUri(InsideSocketUri.UriSchemeUDP, server);
            this.Tx = this.GetTx(this.ServerAddress.IP, this.ServerAddress.Port);
        }
        public virtual void Connect(IPEndPoint ip)
        {
            this.ServerAddress = new InsideSocketUri(InsideSocketUri.UriSchemeUDP, ip);
            this.Tx = this.GetTx(this.ServerAddress.IP, this.ServerAddress.Port);
        }
        public virtual ITx GetTx(string addr) => this.host.GetTx(addr);
        public virtual ITx GetTx(IPEndPoint ip) => this.host.GetTx(ip);
        public virtual ITx GetTx(IPAddress addr, int port) => this.host.GetTx(new IPEndPoint(addr, port));
        private void Rx_Received(UdpData data)
        {
            var packet = new HolePacket();
            if (packet.Verity(data.Data, 0))
            {
                packet.Source = data.IP;
                packet.Read(data.Data);
                this.OnPacketReceived(packet);
                PacketReceived?.Invoke(packet);
            }
        }
        private void Holed_Received(UdpData data)
        {
            Received?.Invoke(data);
        }

        protected virtual void OnRun()
        {
        }
        protected virtual void OnPacketReceived(HolePacket packet)
        {
        }
        public virtual void ResolvePacket()
        {
            lock (this)
            {
                if (this.host == null || this.is_resolving) return;
                this.is_resolving = true;
            }
            this.host.Rx.Received -= Holed_Received;
            this.host.Rx.Received += Rx_Received;
        }

        public virtual void UnResolvePacket()
        {
            lock (this)
            {
                if (this.host == null || !this.is_resolving) return;
                this.is_resolving = false;
            }
            this.host.Rx.Received -= Rx_Received;
            this.host.Rx.Received += Holed_Received;
        }
        protected virtual UdpHoleCore RunAny()
        {
            var core = new UdpHoleCore();
            if (this.Address.IP.GetAddressBytes().Length == 4)
            {
                core.Run("0.0.0.0:0");
            }
            else
            {
                core.Run("[::]:0");
            }
            return core;
        }

        public virtual void Dispose()
        {
            lock (this)
            {
                if (is_disposed) return;
                is_disposed = true;
            }
        }
        protected virtual void I_am_server() { is_server = true; }
    }
}
