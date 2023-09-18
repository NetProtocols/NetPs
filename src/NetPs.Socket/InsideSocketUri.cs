namespace NetPs.Socket
{
    using System;
    using System.Net;
    using System.Text.RegularExpressions;

    public class InsideSocketUri : ISocketUri
    {
        public const string SchemeDelimiter = "://";
        public const string PortDelimiter = ":";
        public const string Ipv6DelimiterLf = "[";
        public const string Ipv6DelimiterRt = "]";

        public const string UriSchemeTCP = "net.tcp";
        public const string UriSchemeUDP = "net.udp";
        public const string UriSchemeICMP = "net.icmp";
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

        public override string ToString()
        {
            //ipv6
            if (Host.Contains(PortDelimiter) && !Host.StartsWith(Ipv6DelimiterLf)) Host = $"{Ipv6DelimiterLf}{Host}{Ipv6DelimiterRt}";
            return $"{Scheme}{SchemeDelimiter}{Host}{PortDelimiter}{Port}";
        }
        public static IPAddress ParseIPAddress(string host)
        {
            switch (host)
            {
                case "[::]":
                case "::":
                    return IPAddress.IPv6Any;
                case "[::1]":
                case "::1":
                    return IPAddress.IPv6Loopback;
                case "127.1":
                case "localhost":
                    return IPAddress.Loopback;
                case "*":
                case "0.0.0.0":
                    return IPAddress.Any;

                default:
                    return IPAddress.Parse(host);
            }
        }
        public static string GetScheme(string uriString)
        {
            var protol = Regex.Match(uriString, @"[a-zA-Z.]+\:\/\/");
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
                    var port = Regex.Match(uriString, @"[1-9]+");
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
            if (ip.Contains("."))
            {
                var b= ip.Split('.');
                if (b.Length > 4) return false;
                for (var i = 0; i < b.Length; i++)
                {
                    var re = Regex.Match(b[i], @"[0-9]+");
                    if (!re.Success) return false;
                    if (re.Value != b[i]) return false;
                }
                return true;
            }else if (ip.Contains(":"))
            {
                ip = ip.Trim('[', ']');
                var b = ip.Split(':');
                if (b.Length > 8) return false;
                for (var i = 0; i < b.Length; i++)
                {
                    if (b[i] == string.Empty) continue;
                    var re = Regex.Match(b[i], @"[0-9a-zA-Z]+");
                    if (!re.Success) return false;
                    if (re.Value != b[i]) return false;
                }
                return true;
            }
            return false;
        }
    }
}
