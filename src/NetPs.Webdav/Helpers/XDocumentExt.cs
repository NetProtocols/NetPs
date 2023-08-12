using System.Xml.Linq;

namespace NetPs.Webdav
{
    internal static class XDocumentExt
    {
        public static XDocument TryParse(string text)
        {
            try
            {
                return XDocument.Parse(text);
            }
            catch
            {
                return null;
            }
        }
    }
}
