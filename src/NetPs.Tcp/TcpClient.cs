namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;

    /// <summary>
    /// Tcp 客户端.
    /// </summary>
    public class TcpClient : TcpRxTx
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TcpClient"/> class.
        /// </summary>
        /// <param name="tcp_config">Socket配置.</param>
        public TcpClient(Action<TcpCore> tcp_config = null): base(tcp_config)
        {
        }

        /// <summary>
        /// Gets or sets 发送完成.
        /// </summary>
        public virtual IObservable<TcpTx> TransportedObservable => this.Tx.TransportedObservable;

        /// <summary>
        /// Gets or sets 接收数据.
        /// </summary>
        public virtual IObservable<byte[]> ReceivedObservable => this.Rx.ReceivedObservable;

        /// <inheritdoc/>
        public override void Dispose()
        {
            base.Dispose();
        }

        public IHub Hub { get; set; }
        /// <summary>
        /// 镜像模式.
        /// </summary>
        /// <param name="address">镜像来源.</param>
        public void StartMirror(string address)
        {
            if (Hub != null) Hub.Close();
            Hub = new MirrorHub(this, address);
            Hub.Start();
        }
    }
}
