namespace NetPs.Udp.Hole
{
    using System;
    public interface IBindUdpHoleCore
    {
        UdpHoleCore Core { get; }

        void BindCore(UdpHoleCore core);
    }
}
