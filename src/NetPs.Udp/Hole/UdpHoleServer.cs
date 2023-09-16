namespace NetPs.Udp.Hole
{
    using NetPs.Socket;
    using NetPs.Socket.Packets;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public class UdpHoleServer : UdpHoleCore
    {
        private bool is_disposed = false;
        private HoleHostRecord records { get; set; }
        private UdpHoleCore fuzhu1 { get; set; }
        private UdpHoleCore fuzhu2 { get; set; }
        private UdpHoleCore fuzhu3 { get; set; }
        private Dictionary<string, bool> fz_callback { get; set; }
        public UdpHoleServer()
        {
            I_am_server();
            this.fz_callback = new Dictionary<string, bool>();
            this.records = new HoleHostRecord();
        }
        protected override void OnRun()
        {
            base.OnRun();
            fuzhu1 = RunAny();
            fuzhu2 = RunAny();
            fuzhu3 = RunAny();
            fuzhu1.PacketReceived += Fz1_PacketReceived;
            fuzhu2.PacketReceived += Fz2_PacketReceived;
            fuzhu3.PacketReceived += Fz3_PacketReceived;
        }

        protected override async void OnPacketReceived(HolePacket packet)
        {
            base.OnPacketReceived(packet);
            packet.IsCallback = true;
            ITx tx;
            switch (packet.Operation)
            {
                case HolePacketOperation.Register:
                    packet.Address = packet.Source;
                    records.Record(packet);
                    tx = GetTx(packet.Source);
                    tx.Transport(packet.GetData());
                    break;
                case HolePacketOperation.Hole:
                    var tag = records.ApplyVerifyTag(packet);
                    var ip = records.FindAddr(packet.Id);
                    var record = HoleFzBag.Create(tag, ip, packet.Source);
                    records.Record(record);
                    await start_fuzhu(tag, fuzhu1, record);
                    //tell_holed(tag, record, this);
                    break;
            }
        }

        private void tell_holed(string tag, HoleFzBag record, UdpHoleCore fz)
        {
            var info = records.GetInfoByTag(tag);
            var pkt = new HolePacket(HolePacketOperation.Hole, info.Id, info.Key);
            pkt.Address = info.Address;
            pkt.Address.Port = record.CurrentPort;
            var tx = this.GetTx(record.RequestAddress);
            tx.Transport(pkt.GetData());

            tx = fz.GetTx(pkt.Address);
            pkt.Address = record.RequestAddress;
            tx.Transport(pkt.GetData());
        }
        private async void Fz1_PacketReceived(HolePacket packet)
        {
            switch (packet.Operation)
            {
                case HolePacketOperation.Fuzhu:
                    end_fuzhu(packet.FuzhuTag, packet.Source, fuzhu1);
                    var record = records.GetFzRecord(packet.FuzhuTag);
                    record.Update(packet.Source);
                    await start_fuzhu(packet.FuzhuTag, fuzhu2, record);
                    //if (record.Verity())
                    //{
                    //    this.tell_holed(packet.FuzhuTag, record, fuzhu1);
                    //}
                    //else
                    //{
                    //    await start_fuzhu(packet.FuzhuTag, fuzhu2, record);
                    //}
                    break;
            }
        }
        private async void Fz2_PacketReceived(HolePacket packet)
        {
            switch (packet.Operation)
            {
                case HolePacketOperation.Fuzhu:
                    end_fuzhu(packet.FuzhuTag, packet.Source, fuzhu2);
                    var record = records.GetFzRecord(packet.FuzhuTag);
                    record.Update(packet.Source);
                    await start_fuzhu(packet.FuzhuTag, fuzhu3, record);
                    //if (record.Verity())
                    //{
                    //    this.tell_holed(packet.FuzhuTag, record, fuzhu2);
                    //}
                    //else
                    //{
                    //    await start_fuzhu(packet.FuzhuTag, fuzhu3, record);
                    //}
                    break;
            }
        }
        private void Fz3_PacketReceived(HolePacket packet)
        {
            switch (packet.Operation)
            {
                case HolePacketOperation.Fuzhu:
                    end_fuzhu(packet.FuzhuTag, packet.Source, fuzhu3);
                    var record = records.GetFzRecord(packet.FuzhuTag);
                    record.Update(packet.Source);
                    if (record.Verity())
                    {
                        this.tell_holed(packet.FuzhuTag, record, fuzhu3);
                    }
                    break;
            }
        }

        private async Task start_fuzhu(string tag, UdpHoleCore fz, HoleFzBag record)
        {
            fz_callback[tag] = false;
            var pkt = new HolePacket();
            pkt.Fuzhu(tag, this.Address.IP, fz.Address.Port);
            var tx = this.GetTx(record.Address, record.CurrentPort);
            tx.Transport(pkt.GetData());
            pkt.IsCallback = true;
            tx = fz.GetTx(record.Address, record.CurrentPort);
            var i = 0;
            while (i++ < 20)
            {
                tx.Transport(pkt.GetData());
                await Task.Delay(10);
                if (fz_callback[tag]) return;
            }
        }
        private void end_fuzhu(string tag, IPEndPoint source, UdpHoleCore fz)
        {
            fz_callback[tag] = true;
            var pkt = new HolePacket();
            pkt.IsCallback = true;
            pkt.Fuzhu(tag);
            var tx = fz.GetTx(source);
            tx.Transport(pkt.GetData());
        }

        public override void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
        }

    }
}
