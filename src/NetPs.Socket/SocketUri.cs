namespace NetPs.Socket
{
    using System;
    using System.Net;
    using System.Text.RegularExpressions;

    public class SocketUri : Uri, ISocketUri
    {
        public const string PortDelimiter = ":";
        public const string UriSchemeUdp = "udp";
        public virtual IPAddress IP { get; protected set; }

        public SocketUri(string uriString) : base(InitializationUriString(uriString))
        {
            Initialization();
        }

        public SocketUri(string protol, string host, int port): base(InitializationUriString(protol, host, port))
        {
            Initialization();
        }

        public SocketUri(string protol, IPEndPoint point) : this(protol, point.Address.ToString(), point.Port) { }

        public SocketUri(string uriString, UriKind uriKind) : base(InitializationUriString(uriString), uriKind)
        {
            Initialization();
        }

        public SocketUri(Uri baseUri, string relativeUri) : base(baseUri, relativeUri)
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            this.IP = ParseIP(Host);
        }

        private static string InitializationUriString(string protol, string host, int port)
        {
            return $"{protol}{Uri.SchemeDelimiter}{host}{PortDelimiter}{port}";
        }

        private static string InitializationUriString(string uriString)
        {
            var protol = GetProtol(uriString);
            if (uriString.StartsWith(protol, StringComparison.OrdinalIgnoreCase))
            {
                return uriString;
            }
            return $"{protol}{Uri.SchemeDelimiter}{uriString}";
        }

        public static string GetProtol(string uriString)
        {
            var protol = Regex.Match(uriString, @"[a-zA-Z.]+\:\/\/");
            if (protol.Success)
            {
                return protol.Value.TrimEnd(':','/','/');
            }

            return Uri.UriSchemeNetTcp;
        }

        public static IPAddress ParseIP(string host)
        {
            switch (host)
            {
                case "[::]":
                case "::":
                    return IPAddress.IPv6Any;
                case "[::1]":
                case "::1":
                    return IPAddress.IPv6Loopback;
                case ":1":
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

        public virtual bool Equal(IPEndPoint ip)
        {
            if (ip == null) return false;
            return ip.Address.Equals(this.IP) && (ip.Port == this.Port || this.Port == 0);
        }

        public virtual bool Equal(ISocketUri host)
        {
            return IP.Equals(host.IP) && (Port == host.Port || Port == 0 || host.Port == 0);
        }
    }
}
