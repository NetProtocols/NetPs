namespace NetPs.Socket
{
    using NetPs.Socket.Eggs;
    using NetPs.Socket.Operations;
    using System;
    using System.Linq;
    using System.Net.Sockets;

    public sealed class Heat_socket : IHeat
    {
        private class hot_SocketCore : SocketCore { }
        private const string hot_uri = "127.0.0.1:80";
        public void Start(IHeatingWatch watch)
        {
            new NetPsSocketException(SocketErrorCode.Success, string.Empty);
            new QueueStream().Dispose();
            new hot_SocketCore().Dispose();
            ArrayTool.Exist(ArrayTool.FindAll(ArrayTool.Empty<int>().ToReadOnly().ToArray(), ar => true), ar => true);
            var uri = new SocketUri(hot_uri);
            watch.Heat_Progress();
            using (var socket = new Socket(uri.IP.AddressFamily, SocketType.Dgram, ProtocolType.Udp))
            {

            }
            watch.Heat_Progress();
            new HostIPList().Load();
            watch.Heat_Progress();
        }
    }
}
