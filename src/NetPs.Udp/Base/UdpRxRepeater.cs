namespace NetPs.Udp.Base
{
    using NetPs.Socket;
    using System;

    public class UdpRxRepeater : UdpRx, IDisposable, IEndTransport, ISpeedLimit
    {
        internal UdpRxRepeater() { }
        public UdpRxRepeater(UdpCore udpCore) : base(udpCore)
        {
        }

        public int Limit => throw new NotImplementedException();

        public long LastTime => throw new NotImplementedException();

        public void SetLimit(int value)
        {
        }

        public void WhenTransportEnd(IDataTransport transport)
        {
        }
    }
}
