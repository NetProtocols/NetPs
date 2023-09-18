namespace NetPs.Socket
{
    using NetPs.Socket.Eggs;
    using NetPs.Socket.Icmp;
    using NetPs.Socket.Operations;
    using NetPs.Socket.Packets;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class Heat_socket : IHeat
    {
        private class hot_SocketCore : SocketCore
        {
            protected override void OnClosed()
            {
                throw new NotImplementedException();
            }

            protected override void OnLosed()
            {
                throw new NotImplementedException();
            }
        }
        private const string hot_uri = "127.0.0.1:80";
        public void Start(IHeatingWatch watch)
        {
            new NetPsSocketException(SocketErrorCode.Success, string.Empty);
            new QueueStream().Dispose();
            new hot_SocketCore().Dispose();
            ArrayTool.Exist(ArrayTool.FindAll(ArrayTool.Empty<int>().ToReadOnly().ToArray(), ar => true), ar => true);
            new InsideSocketUri(hot_uri);
            new InsideSocketUri();
            watch.Heat_Progress();
            new HostIPList().Load();
            watch.Heat_Progress();
            new PingClient().Dispose();
            new PingV6Client().Dispose();
            new PingPacket(PingPacket.DATA_32, PingPacketKind.Request)
            {
                Address = IPAddress.Parse("::1")
            };
            new HolePacket();
            Task.Factory.StartNew(() => { });
            var e = new EventWaitHandle(true, EventResetMode.ManualReset);
            e.Set();
            e.Close();
            on_ThreadPool();
            watch.Heat_Progress();
        }
        static void on_ThreadPool()
        {
            ThreadPool.QueueUserWorkItem(ThreadProc);
            Thread.Sleep(0);
        }

        static void ThreadProc(Object stateInfo)
        {
        }
    }
}
