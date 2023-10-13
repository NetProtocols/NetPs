namespace NetPs.Socket
{
    using System;
    using System.Threading;

    public delegate void HubExceptionHandler(Exception e);
    public static class Hub
    {
        public static event HubExceptionHandler Exceptioned;

        public static void ThrowException(Exception e)
        {
            if (Hub.Exceptioned != null) Exceptioned.Invoke(e);
        }
    }
    public abstract class HubBase
    {
        private static int inner_id = 1;
        private bool is_closed = false;
        public event EventHandler Closed;
        private readonly int id;
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
}
