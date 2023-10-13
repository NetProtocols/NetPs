using System;
using System.Threading;

namespace NetPs.Socket
{
    public interface IHub : IDisposable
    {

        event EventHandler Closed;
        /// <summary>
        /// 标识.
        /// </summary>
        int ID { get; }
        void Close();
        void Start();
    }
}
