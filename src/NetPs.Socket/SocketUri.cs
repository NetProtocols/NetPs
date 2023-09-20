namespace NetPs.Socket
{
    using System;
    using System.Net;

    public class SocketUri : Uri, ISocketUri
    {
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
            this.IP = InsideSocketUri.ParseIPAddress(Host);
        }

        private static string InitializationUriString(string protol, string host, int port)
        {
            //ipv6
            if (host.Contains(InsideSocketUri.PortDelimiter) && host[0] != InsideSocketUri.Ipv6DelimiterLf) host = $"{InsideSocketUri.Ipv6DelimiterLf}{host}{InsideSocketUri.Ipv6DelimiterRt}";
            return $"{protol}{InsideSocketUri.SchemeDelimiter}{host}{InsideSocketUri.PortDelimiter}{port}";
        }

        private static string InitializationUriString(string uriString)
        {
            var protol = InsideSocketUri.GetScheme(uriString);
            if (uriString.StartsWith(protol, StringComparison.OrdinalIgnoreCase))
            {
                return uriString;
            }
            return $"{protol}{InsideSocketUri.SchemeDelimiter}{uriString}";
        }
    }
}
