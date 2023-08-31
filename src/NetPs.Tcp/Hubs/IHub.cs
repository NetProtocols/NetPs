using System;
using System.Collections.Generic;

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
        private static long id = 1;
        private bool is_closed = false;
        
        public event EventHandler Closed;

        public void Close()
        {
            lock (this)
            {
                if (is_closed) return;
                this.is_closed = true;
            }
            Closed?.Invoke(this, EventArgs.Empty);
        }

        protected abstract void OnClosed();

        protected static long GetId()
        {
            return id++;
        }
    }
    public interface IHub : IDisposable
    {

        event EventHandler Closed;
        /// <summary>
        /// 标识.
        /// </summary>
        long ID { get; }
        void Close();
        void Start();
    }
}
