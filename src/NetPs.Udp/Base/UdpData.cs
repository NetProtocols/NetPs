namespace NetPs.Udp
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    [DataContract]
    public struct UdpData
    {
        [DataMember]
        public IPEndPoint IP { get; set; }
        [DataMember]
        public byte[] Data { get; set; }
    }
}
