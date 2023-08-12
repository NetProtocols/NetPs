using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace NetPs.Socket
{
    public class SocketUri : Uri
    {
        public virtual IPAddress IP { get; protected set; }

        public SocketUri(string uriString) : base(InitializationUriString(uriString))
        {
            Initialization();
        }

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

        private static string InitializationUriString(string uriString)
        {
            var protol = GetProtol(uriString);
            if (uriString.StartsWith(protol, StringComparison.OrdinalIgnoreCase))
            {
                return uriString;
            }
            return $"{protol}://{uriString}";
        }

        public static string GetProtol(string uriString)
        {
            var protol = Regex.Match(uriString, @"[a-zA-Z]+\:\/\/");
            if (protol.Success)
            {
                return protol.Value.TrimEnd("://".ToCharArray());
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
    }
}
