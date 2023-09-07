namespace NetPs.Tcp.Hole
{
    using NetPs.Socket;
    using System;
    using System.Collections.Generic;
    using System.Net;
    public interface IHoleServerEvents
    {
        void OnHoleReceived(HolePacket packet, TcpClient client);
    }
    public class TcpHoleServer : IDisposable, ITcpServerEvents, IRxEvents
    {
        private IHoleServerEvents events { get; set; }
        private TcpServer server { get; set; }
        private Dictionary<string, HolePacket> hosts { get; set; }
        public IPEndPoint Address => this.server.IPEndPoint;
        public TcpHoleServer()
        {
            server = new TcpServer();
            server.BindEvents(this);
            this.hosts = new Dictionary<string, HolePacket>();
        }

        public void BindEvents(IHoleServerEvents events)
        {
            this.events = events;
        }

        public virtual void Run(string uri)
        {
            server.Run(uri);
        }

        public void OnAccepted(ITcpServer tcpServer, ITcpClient tcpClient)
        {
            tcpClient.BindRxEvents(this);
            tcpClient.StartReceive();
        }

        public void OnClosed(ITcpServer tcpServer)
        {
        }

        public void OnConfiguration(ITcpServer tcpServer)
        {
        }

        public void OnConfiguration(TcpCore core)
        {
        }

        public void OnListened(ITcpServer tcpServer)
        {
        }

        public void OnSocketLosed(ITcpServer tcpServer, ITcpClient tcpClient)
        {
        }

        public void OnReceiving(IRx rx)
        {
        }

        public void OnReceived(IRx rx)
        {
            if (rx is TcpRx tcpRx)
            {
                if (tcpRx.TcpCore is TcpClient client)
                {
                    var packet = new HolePacket();
                    var len = packet.Read(rx.Buffer);
                    switch (packet.Operation)
                    {
                        case HolePacketOperation.Register:
                            packet.Address = rx.RemoteAddress;
                            hosts[packet.Id] = packet;
                            break;
                        case HolePacketOperation.GetId:
                            if (hosts.ContainsKey(packet.Id))
                            {
                                packet.IsCallback = true;
                                packet.Address = hosts[packet.Id].Address;
                                client.Transport(packet.GetData());
                            }
                            break;
                        case HolePacketOperation.HoleReady:
                            break;
                    }
                    this.events?.OnHoleReceived(packet, client);
                }
            }
        }

        public void OnDisposed(IRx rx)
        {
        }

        public void Dispose()
        {
            if (!this.server.IsDisposed)
            {
                this.server.Dispose();
            }
        }
    }
}
