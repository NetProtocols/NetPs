namespace NetPs.Tcp.Hole
{
    using System;

    /// <summary>
    /// 内网穿透客户端
    /// </summary>
    /// <remarks>
    /// * 需要管理员权限
    /// </remarks>
    public class TcpHoleClient : TcpServer
    {
        public virtual TcpClient Clone()
        {
            var client = new TcpClient(core =>
            {
                core.AllowReuseAddress();
            });
            client.Bind(Address);
            return client;
        }

        protected override void OnConfiguration()
        {
            base.OnConfiguration();
            AllowReuseAddress();
        }
    }
}
