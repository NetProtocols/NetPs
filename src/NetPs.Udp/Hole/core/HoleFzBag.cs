namespace NetPs.Udp.Hole
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    /// <summary>
    /// 辅助数据空间
    /// </summary>
    public struct HoleFzBag
    {
        public string Tag { get; set; }
        public IPAddress Address { get; set; }
        public int CurrentPort { get; set; }
        public IList<int> Ports { get; set; }
        public IPEndPoint RequestAddress { get; set; }
        public static HoleFzBag Create(string tag, IPEndPoint ip, IPEndPoint request)
        {
            var bag = new HoleFzBag
            { Tag = tag, Address = ip.Address, CurrentPort = ip.Port, Ports = new List<int>(3) };
            bag.RequestAddress = request;
            return bag;
        }

        public void Update(IPEndPoint ip)
        {
            this.Ports.Add(ip.Port);
        }

        public bool Verity()
        {
            var pre = this.CurrentPort;
            for (var i = 0; i<Ports.Count;i++)
            {
                if (pre == Ports[i])
                {
                    this.CurrentPort = pre;
                    return true;
                }
                pre = Ports[i];
            }

            return false;
        }
    }
}
