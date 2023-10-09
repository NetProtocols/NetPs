namespace NetPs.Socket
{
    using System;
    using System.Net;
    public class IPAddressWithMask : IPAddress
    {
        public virtual byte[] Mask { get; private set; }
        public virtual byte[] NetMask { get; private set; }
        public IPAddressWithMask(byte[] address, byte[] mask) : base(address)
        {
            this.ResetMask(mask);
        }

        public virtual bool IsBroadcast()
        {
            var bytes = this.GetAddressBytes();
            if (!CheckNetMask(NetMask)) return IPAddress.Broadcast.Equals(this);
            for (var i = 0; i < NetMask.Length; i++)
            {
                if (bytes[i] != (bytes[i] | NetMask[i])) return false;
            }
            return true;
        }
        public virtual void ResetMask(byte[] mask)
        {
            this.Mask = mask;
            this.NetMask = ReverseMask(mask);
        }
        public static byte[] ReverseMask(byte[] mask)
        {
            var bytes = new byte[mask.Length];
            for (var i = 0; i < mask.Length; i++) bytes[i] = (byte)~mask[i];
            return bytes;
        }
        public static bool CheckNetMask(byte[] mask)
        {
            var times = 0;
            for (var i = 0; i < mask.Length; i++)
            {
                var c = mask[i];
                for (var j = 0; j < 8; j++)
                {
                    if (((c >> (7 - j)) & 1) == 0)
                    {
                        if (times > 0)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        times++;
                    }
                }
            }
            return times > 1;
        }
    }
}
