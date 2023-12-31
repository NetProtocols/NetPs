﻿namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net.Sockets;

    public class TcpClient: TcpClientFactory<TcpTx, TcpRx>
    {
        public TcpClient(TcpConfigFunction tcp_config = null) : base(tcp_config) { }
        public TcpClient(ITcpClientEvents events) : base(events) { }
        public TcpClient(Socket socket) : base(socket) { }
        /// <summary>
        /// 镜像模式.
        /// </summary>
        /// <param name="address">镜像来源.</param>
        public void StartMirror(string address, int limit = -1)
        {
            this.StartHub(new MirrorHub<TcpRepeaterClient>(this, address, limit));
        }
    }
}
