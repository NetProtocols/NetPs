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
        private static object locker = new object();
        
        public event EventHandler Closed;

        public void Close()
        {
            Closed?.Invoke(this, EventArgs.Empty);
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
        /// <summary>
        /// 标识.
        /// </summary>
        long ID { get; }
        void Close();
        void Start();
    }
}
