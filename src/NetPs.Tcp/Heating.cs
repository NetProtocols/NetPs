namespace NetPs.Socket
{
    using NetPs.Socket.Eggs;
    using NetPs.Tcp;
    using System;

    /// <summary>
    /// 加热类
    /// </summary>
    public sealed class Heat_tcp : ITcpServerConfig, IHeat
    {
        private readonly byte[] test_data = { 0 } ;
        public void Start(IHeatingWatch watch)
        {
            var serv = new TcpServer(this);
            watch.Heat_Progress();
            var client = new TcpClient();
            client.Disposables.Add(client.ConnectedObservable.Subscribe(ip =>
            {
                watch.Heat_Progress();
                client.Transport(test_data);
                serv.Dispose();
            }));
            client.Disposables.Add(client.TransportedObservable.Subscribe(tx => client.Dispose()));
            client.Connect($"127.0.0.1:{serv.Address.Port}");
            watch.Heat_Progress();
            var static_val = Consts.ReceiveBytes;
            static_val = Consts.SocketPollTime;
            static_val = Consts.TransportBytes;
            watch.Heat_Progress();
        }

        public string BandAddress => "0.0.0.0:0";

        public bool TcpAccept(TcpServer server, TcpClient client)
        {
            return true;
        }

        public void TcpConfigure(TcpCore core)
        {
        }

        public void TcpReceive(byte[] data, TcpClient tcp)
        {
        }
    }
}
