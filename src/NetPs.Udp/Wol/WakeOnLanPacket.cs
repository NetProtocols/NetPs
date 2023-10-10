namespace NetPs.Udp.Wol
{
    using NetPs.Socket;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public class WakeOnLanPacket : IPacket
    {
        public string Mac { get; set; }
        public WakeOnLanPacket()
        {
        }
        public WakeOnLanPacket(string mac)
        {
            Mac = mac;
        }

        public byte[] GetData()
        {
            var mac_len = MacBytes().Count();
            var data = new byte[6 + 16*mac_len];
            var i = 6;
            while (i-- > 0) data[i] = 0xff;
            var j = -1;
            foreach (var b in MacBytes())
            {
                j++;
                i = 16;
                while (i-- > 0) data[6 + i * mac_len + j] = b;
            }
            return data;
        }
        public IEnumerable<byte> MacBytes()
        {
            var re = new Regex("[0-9a-fA-F]{2}");
            foreach (Match match in re.Matches(Mac))
            {
                if (match.Success)
                {
                    yield return byte.Parse(match.Value, System.Globalization.NumberStyles.HexNumber);
                }
            }
        }
        public static bool IsMacAddress(string mac)
        {
            var times = 0;
            var re = new Regex("[0-9a-fA-F]{2}");
            foreach (Match match in re.Matches(mac))
            {
                if (match.Success)
                {
                    times++;
                }
            }
            return times == 6;
        }

        public void SetData(byte[] data, int offset)
        {
            if (!Verity(data, offset)) throw new ArgumentException("WakeOnLanPacket data incorrect");
            var mac_len = (data.Length - offset) / 16;
            var builder = new StringBuilder();
            var i = -1;
            while (++i < mac_len)
            {
                if (i != 0) builder.Append("-");
                builder.Append(string.Format("{0:X2}", data[offset + i + 6]));
            }
            Mac = builder.ToString();
        }

        public bool Verity(byte[] data, int offset)
        {
            var i = 6;
            while (i-- > 0 && data[i + offset] == 0xff) ;
            if (i == -1)
            {
                if (data.Length - 6 >= 16)
                {
                    var mac_len = (data.Length - offset - 6) / 16;
                    var j = mac_len;
                    while (j-- > 0)
                    {
                        i = 16;
                        var b = data[j + offset + 6];
                        while (i-- > 1 && b == data[offset + 6 + i * mac_len + j]) ;
                        if (i != 0) return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
