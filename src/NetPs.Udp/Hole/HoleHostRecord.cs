namespace NetPs.Udp.Hole
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using NetPs.Socket.Packets;

    /// <summary>
    /// Hole 主机记录
    /// </summary>
    public class HoleHostRecord
    {
        private Dictionary<string, HoleFzBag> fz_bags { get; set; }
        private Dictionary<string, HolePacket> hosts { get; set; }
        public HoleHostRecord()
        {
            this.hosts = new Dictionary<string, HolePacket>();
            this.fz_bags = new Dictionary<string, HoleFzBag>();
        }
        public string ApplyVerifyTag(HolePacket packet)
        {
            return $"J{packet.Id}";
        }
        public string GetIdByTag(string tag)
        {
            return tag.Substring(1);
        }
        public virtual HolePacket GetInfoByTag(string tag)
        {
            var id = GetIdByTag(tag);
            if (ContainsHoleId(id))
            {
                return hosts[id];
            }
            throw new Exception($"not info by {tag}");
        }
        public virtual void Record(string tag, IPEndPoint ip)
        {
            if (this.fz_bags.ContainsKey(tag))
            {
                this.fz_bags[tag].Update(ip);
            }
        }
        public virtual void Record(HoleFzBag bag)
        {
            this.fz_bags[bag.Tag] = bag;
        }
        public virtual HoleFzBag GetFzRecord(string tag)
        {
            if (this.fz_bags.ContainsKey(tag))
            {
                return this.fz_bags[tag];
            }
            throw new Exception($"fz_bags notchild {tag}");
        }

        public virtual void Record(HolePacket packet)
        {
            hosts[packet.Id] = packet;
        }

        public virtual bool ContainsHoleId(string id)
        {
            return this.hosts.ContainsKey(id);
        }
        public virtual bool ContainsTag(string tag)
        {
            return this.fz_bags.ContainsKey(tag);
        }
        public virtual IPEndPoint FindAddr(string id)
        {
            return this.hosts[id].Address;
        }
    }
}
