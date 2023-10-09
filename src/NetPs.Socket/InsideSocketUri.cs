namespace NetPs.Socket
{
    using System;
    using System.Net;
    using System.Text.RegularExpressions;

    public class InsideSocketUri : ISocketUri
    {
        public const string UriSchemeTCP = "net.tcp";
        public const string UriSchemeUDP = "net.udp";
        public const string UriSchemeICMP = "net.icmp";
        public const string UriSchemeUnknown = "net.unknown";

        internal const string SchemeDelimiter = "://";
        internal const string PortDelimiter = ":";
        internal const char Ipv6DelimiterLf = '[';
        internal const char Ipv6DelimiterRt = ']';
        internal const char IPv4Delimiter = '.';
        internal const char IPv6Delimiter = ':';
        internal const string IPv6Any = "::";
        internal const string IPv6Any2 = "[::]";
        internal const string IPv6Loopback = "::1";
        internal const string IPv6Loopback2 = "[::1]";
        internal const string IPv4Loopback = "127.1";
        internal const string IPv4Loopback2 = "localhost";
        internal const string IPv4Any = "*";
        internal const string IPv4Any2 = "0.0.0.0";
        internal const string SchemeRegex = @"[a-zA-Z.]+\:\/\/";
        internal const string NumberRegex = @"[0-9]+";
        internal const string HexRegex = @"[0-9a-zA-Z]+";

        internal InsideSocketUri()
        {
        }
        public InsideSocketUri(string uri) : this(GetScheme(uri), uri)
        {
        }
        public InsideSocketUri(string protol, string uri)
        {
            this.Scheme = protol;
            this.Host = GetHost(uri);
            this.IP = ParseIPAddress(this.Host);
            this.Port = GetPort(uri);
        }
        public InsideSocketUri(ISocketUri uri, int port)
        {
            this.Scheme = uri.Scheme;
            this.Host = uri.Host;
            this.IP = uri.IP;
            this.Port = port;
        }
        public InsideSocketUri(string protol, string host, int port)
        {
            this.Scheme = protol;
            this.Host = host;
            this.IP = ParseIPAddress(host);
            this.Port = port;
        }
        public InsideSocketUri(string protol, IPAddress ip, int port)
        {
            this.Scheme = protol;
            this.Host = ip.ToString();
            this.IP = ip;
            this.Port = port;
        }
        public InsideSocketUri(string protol, IPEndPoint point)
        {
            this.Scheme = protol;
            this.Host = point.Address.ToString();
            this.IP = point.Address;
            this.Port = point.Port;
        }
        public string Scheme { get; private set; }

        public IPAddress IP { get; private set; }

        public string Host { get; private set; }

        public int Port { get; private set; }

        public virtual void SetPort(int port)
        {
            this.Port = port;
        }
        public override string ToString()
        {
            //ipv6
            if (Host.Contains(PortDelimiter) && Host[0] != Ipv6DelimiterLf) Host = $"{Ipv6DelimiterLf}{Host}{Ipv6DelimiterRt}";
            return $"{Scheme}{SchemeDelimiter}{Host}{PortDelimiter}{Port}";
        }
        public static IPAddress ParseIPAddress(string host)
        {
            switch (host)
            {
                case IPv6Any:
                case IPv6Any2:
                    return IPAddress.IPv6Any;
                case IPv6Loopback:
                case IPv6Loopback2:
                    return IPAddress.IPv6Loopback;
                case IPv4Loopback:
                case IPv4Loopback2:
                    return IPAddress.Loopback;
                case IPv4Any:
                case IPv4Any2:
                    return IPAddress.Any;

                default:
                    return IPAddress.Parse(host);
            }
        }
        public static string GetScheme(string uriString)
        {
            var protol = Regex.Match(uriString, SchemeRegex);
            if (protol.Success)
            {
                return protol.Value.Substring(0, protol.Value.Length -3);
            }

            return UriSchemeTCP;
        }
        public static string GetHost(string uriString)
        {
            var scheme = GetScheme(uriString);
            uriString = uriString.TrimStart(scheme.ToCharArray());
            if (uriString.Length > 0)
            {
                var ix = uriString.LastIndexOf(PortDelimiter);
                if (ix > 0)
                {
                    uriString = uriString.Substring(0, ix);
                }
                return uriString;
            }

            return string.Empty;
        }

        public static int GetPort(string uriString)
        {
            var scheme = GetScheme(uriString);
            uriString = uriString.TrimStart(scheme.ToCharArray());
            if (uriString.Length > 0)
            {
                var ix = uriString.LastIndexOf(PortDelimiter);
                if (ix > 0)
                {
                    uriString = uriString.Substring(ix);
                    var port = Regex.Match(uriString, NumberRegex);
                    if (port.Success)
                    {
                        return int.Parse(port.Value);
                    }
                }
            }

            return 0;
        }

        public static bool IsIPAddress(string ip)
        {
            if (ip == IPv4Loopback2) return true;
            else if (Array.IndexOf(ip.ToCharArray(), IPv4Delimiter, 0) != -1)
            {
                var b= ip.Split(IPv4Delimiter);
                if (b.Length > 4) return false;
                for (var i = 0; i < b.Length; i++)
                {
                    var re = Regex.Match(b[i], NumberRegex);
                    if (!re.Success) return false;
                    if (re.Value != b[i]) return false;
                }
                return true;
            }
            else if (Array.IndexOf(ip.ToCharArray(), IPv6Delimiter, 0) != -1)
            {
                ip = ip.Trim(Ipv6DelimiterLf, Ipv6DelimiterRt);
                var b = ip.Split(IPv6Delimiter);
                if (b.Length > 8) return false;
                for (var i = 0; i < b.Length; i++)
                {
                    if (b[i] == string.Empty) continue;
                    var re = Regex.Match(b[i], HexRegex);
                    if (!re.Success) return false;
                    if (re.Value != b[i]) return false;
                }
                return true;
            }
            return false;
        }
    }
}
