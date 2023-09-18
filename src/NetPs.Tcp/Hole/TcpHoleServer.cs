namespace NetPs.Tcp.Hole
{
    using NetPs.Socket;
    using NetPs.Socket.Packets;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    public class TcpHoleServer : IDisposable, ITcpServerEvents, IRxEvents
    {
        private IHoleEvents events { get; set; }
        private TcpServer server { get; set; }
        private Dictionary<string, HolePacket> hosts { get; set; }
        public IPEndPoint Address => this.server.IPEndPoint;
        public TcpHoleServer()
        {
            server = new TcpServer();
            server.BindEvents(this);
            this.hosts = new Dictionary<string, HolePacket>();
        }

        public void BindEvents(IHoleEvents events)
        {
            this.events = events;
        }

        public virtual void Run(string uri)
        {
            server.Run(uri);
        }
        public virtual TcpClient Clone()
        {
            var client = new TcpClient(core =>
            {
                core.SetReuseAddress(true);
            });
            client.Bind(server.Address);
            return client;
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
            (tcpServer as TcpServer).SetReuseAddress(true);
        }

        public void OnConfiguration(TcpCore core)
        {
        }

        public void OnListened(ITcpServer tcpServer)
        {
        }

        public void OnSocketLosed(ITcpServer tcpServer, ITcpClient tcpClient)
        {
            Console.WriteLine($"S Lose");
        }

        public void OnReceiving(IRx rx)
        {
        }

        public void ConnectTo(string uri)
        {
            var c1 = Clone();
            c1.ConnectedObservable.Subscribe(o =>
            {
                Console.WriteLine($"S Connect {uri}");
            });
            c1.LoseConnectedObservable.Subscribe(o =>
            {
                Console.WriteLine($"S Lose {uri}");
            });
            c1.Connect(uri);
            Console.WriteLine($"S Connect {uri}");
            cs.Add(c1);
        }
        List<TcpClient> cs = new List<TcpClient>();
        public void OnReceived(IRx rx)
        {
            if (rx is TcpRx tcpRx)
            {
                if (tcpRx.Core is TcpClient client)
                {
                    var packet = new HolePacket();
                    var len = packet.Read(rx.Buffer);
                    switch (packet.Operation)
                    {
                        case HolePacketOperation.Register:
                            packet.Address = rx.RemoteAddress;
                            hosts[packet.Id] = packet;
                            //client.Close();
                            break;
                        case HolePacketOperation.Hole:
                        case HolePacketOperation.HoleCallback:
                            packet.IsCallback = true;
                            //根据Address判断
                            if (hosts.ContainsKey(packet.Id))
                            {
                                var id_address = hosts[packet.Id].Address;
                                var dst_client = this.server.Connects.FirstOrDefault(con => con.RemoteAddress.Equal(id_address));
                                if (dst_client != null)
                                {
                                    packet.Address = client.RemoteIPEndPoint;
                                    dst_client.TransportedObservable.Subscribe(tx =>
                                    {
                                        dst_client.Lose();
                                    });
                                    dst_client.Transport(packet.GetData());
                                }
                                packet.Address = id_address;
                                client.TransportedObservable.Subscribe(tx =>
                                {
                                    client.Lose();
                                });
                                client.Transport(packet.GetData());

                            }
                            else
                            {
                                client.Lose();
                            }
                            break;
                    }
                    this.events?.OnReceivedPacket(packet, client);
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
