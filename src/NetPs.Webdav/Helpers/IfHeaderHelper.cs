namespace NetPs.Webdav
{
    internal static class IfHeaderHelper
    {
        public static string GetHeaderValue(string lockToken)
        {
            return $"(<{lockToken}>)";
        }
    }
}
