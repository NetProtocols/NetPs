namespace NetPs.Tcp
{
    using System;
    using System.Net;

    public abstract class BindTcpCore : IBindTcpCore
    {
        public virtual TcpCore Core { get; private set; }
        public virtual IPEndPoint RemoteAddress => this.Core.RemoteIPEndPoint;
        public virtual void BindCore(ITcpClient client)
        {
            if (client is TcpCore core)
            {
                this.BindCore(core);
            }
        }
        public virtual void BindCore(TcpCore core)
        {
            this.Core = core;
        }
    }
}
