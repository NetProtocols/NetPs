namespace NetPs.Udp.DNS
{
    using System;
    public static class Punycode
    {
        /// <summary>
        /// Punycode编码
        /// </summary>
        /// <param name="url">网络地址</param>
        /// <returns></returns>
        public static string Encode(string url) => X_PunycodeEncode(url);
        /// <summary>
        /// Punycode解码
        /// </summary>
        /// <param name="url">网络地址</param>
        /// <returns></returns>
        public static string Decode(string url) => X_PunycodeDecode(url);
        private static string X_PunycodeEncode(string url)
        {
            return Nunycode.Punycode.ToAscii(url);
        }

        private static string X_PunycodeDecode(string url)
        {
            return Nunycode.Punycode.ToUnicode(url);
        }
    }
}
