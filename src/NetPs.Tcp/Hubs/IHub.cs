using System;
using System.Collections.Generic;

namespace NetPs.Tcp
{
    public delegate bool HubExceptionHandler(Exception e);
    public abstract class HubBase {
        private static long id = 1;
        private static object locker = new object();
        
        public event EventHandler Closed;
        public event HubExceptionHandler Exceptioned;

        public void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }

        public bool ThrowException(Exception e)
        {
            if (Exceptioned != null) return true;
            var ok = Exceptioned.Invoke(e);
            return ok;
        }

        protected static long GetId()
        {
            lock (locker)
            {
                return id++;
            }
        }
    }
    public interface IHub : IDisposable
    {

        event EventHandler Closed;
        event HubExceptionHandler Exceptioned;
        /// <summary>
        /// 标识.
        /// </summary>
        long ID { get; }
        void Close();
        void Start();
        bool ThrowException(Exception e);
    }
}
