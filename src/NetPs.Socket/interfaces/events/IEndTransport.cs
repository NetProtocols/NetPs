namespace NetPs.Socket
{
    using System;
    /// <summary>
    /// 数据传输结束事件
    /// </summary>
    public interface IEndTransport
    {
        void WhenTransportEnd(IDataTransport transport);
    }
}
