namespace NetPs.Socket
{
    using System;
    using System.Diagnostics;
    public partial class Utils
    {
#if NETSTANDARD
        /// <summary>
        /// 打开Edge浏览器
        /// </summary>
        public static void OpenBrowser_Edge(string uri)
        {
            Process.Start(new ProcessStartInfo(@$"microsoft-edge:{uri}") { UseShellExecute = true });
        }
#endif
    }
}
