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
    }
}
