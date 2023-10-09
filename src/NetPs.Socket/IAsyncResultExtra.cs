namespace NetPs.Socket
{
    using System;

    public static class IAsyncResultExtra
    {
        public static void Wait(this IAsyncResult asyncResult)
        {
            if (asyncResult == null) return;
            if (!asyncResult.IsCompleted && asyncResult.CompletedSynchronously)
            {
                asyncResult.AsyncWaitHandle.WaitOne();
            }
        }
        public static void Wait(this IAsyncResult asyncResult, int timeout)
        {
            if (asyncResult == null) return;
            if (!asyncResult.IsCompleted)
            {
                asyncResult.AsyncWaitHandle.WaitOne(timeout, false);
            }
        }
        public static void Close(this IAsyncResult asyncResult)
        {
            if (asyncResult == null) return;
#if NETSTANDARD
            if (asyncResult.AsyncWaitHandle.SafeWaitHandle.IsClosed) return;
#endif
            asyncResult.AsyncWaitHandle.Close();
        }
    }
}
