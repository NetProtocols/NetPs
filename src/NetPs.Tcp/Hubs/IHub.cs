using System;
using System.Threading;

namespace NetPs.Tcp
{
    public delegate void HubExceptionHandler(Exception e);
    public static class Hub
    {
        public static event HubExceptionHandler Exceptioned;

        public static void ThrowException(Exception e)
        {
            if (Hub.Exceptioned != null) Exceptioned.Invoke(e);
        }
    }
    public abstract class HubBase {
        private static int inner_id = 1;
        private bool is_closed = false;
        public event EventHandler Closed;
        public readonly int id;
        public HubBase()
        {
            id = GetId();
        }
        public int ID => this.id;
        public void Close()
        {
            lock (this)
            {
                if (is_closed) return;
                this.is_closed = true;
            }
            this.OnClosed();
            Closed?.Invoke(this, EventArgs.Empty);
        }

        protected abstract void OnClosed();
        protected virtual int GetId()
        {
            return Interlocked.Increment(ref inner_id);
        }
    }
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
