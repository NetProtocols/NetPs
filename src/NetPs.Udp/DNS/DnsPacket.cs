using NetPs.Socket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace NetPs.Udp.DNS
{
    public class DnsQuestion
    {
        public string Name;
        public string Name_punycode;
        public ushort Type;
        public ushort Class;
    }

    public class DnsDataResult : DnsQuestion
    {
        public int LiveTime;
        public ushort DataLength;
        public byte[] Data;
    }

    public class DnsAnswer : DnsDataResult
    {
        public IPAddress Address;
        public string CNAME;
    }

    public class DnsAuthoritativeNameserver : DnsDataResult
    {
        public string PrimaryNameserver;
        public string ResponsibleAuthorityMailbox;
        public long SerialNumber;
        public int RefreshInterval;
        public int RetryInterval;
        public int ExpireLimit;
        public int MinimumTTL;
    }

    public class DnsAdditional : DnsDataResult
    {
        public IPAddress Address;
    }
    public class DnsPacket : IPacket
    {
        public const ushort Flag_standard_query = 0x0100;
        public const ushort Flag_standard_query_response = 0x8180;
        public const ushort Flag_standard_query_response_err = 0x8183;
        public const ushort Flag_standard_query_response_NOTIMPLEMENTED = 0x8184;
        public const ushort Type_AAAA = 0x001c;
        public const ushort Type_A = 0x0001;
        public const ushort Type_NS = 0x0002;
        public const ushort Type_CNAME = 0x0005;
        public const ushort Type_SOA = 0x0006;
        public const ushort Type_WKS = 0x000b;
        public const ushort Type_PTR = 0x000c;
        public const ushort Type_HINFO = 0x000d;
        public const ushort Type_MX = 0x000f;
        public const ushort Type_AXFR = 0x00fc;
        public const ushort Type_ANY = 0x00ff;
        public const ushort Class_IN = 0x0001;
        public IPEndPoint IPEndPoint;
        public ushort TransactionID;
        public ushort Flags = Flag_standard_query;
        public ushort Questions = 1;
        public ushort AnswerRRs = 0;
        public ushort AuthorityRRs = 0;
        public ushort AdditionalRRs = 0;

        public IList<DnsQuestion> Queries;
        public IList<DnsAnswer> Answers;
        public IList<DnsAuthoritativeNameserver> AuthoritativeNameservers;
        public IList<DnsAdditional> Additionals;

        public DnsPacket(params DnsQuestion[] queries)
        {
            Queries = queries;
        }

        public DnsPacket(byte[] data)
        {
            Answers = new List<DnsAnswer>();
            AuthoritativeNameservers = new List<DnsAuthoritativeNameserver>();
            Additionals = new List<DnsAdditional>();
            using (var ms = new MemoryStream(data))
            {
                var reader = new BinaryReader(ms);
                TransactionID = Read_ushort(reader);
                Flags = Read_ushort(reader);
                if (Flag_standard_query_response != Flags) return; 
                Questions = Read_ushort(reader);
                AnswerRRs = Read_ushort(reader);
                AuthorityRRs = Read_ushort(reader);
                AdditionalRRs = Read_ushort(reader);
                Queries = new List<DnsQuestion>();
                while (Queries.Count < Questions)
                {
                    var question = new DnsQuestion();
                    Read_DnsQuestion(question, reader);
                    Queries.Add(question);
                }
                while (Answers.Count < AnswerRRs)
                {
                    var answer = new DnsAnswer();
                    Read_DnsAnswer(answer, reader);
                    Answers.Add(answer);

                    switch (answer.Type)
                    {
                        case Type_A:
                        case Type_AAAA:
                            answer.Address = new IPAddress(answer.Data);
                            break;
                        case Type_CNAME:
                            using (var ms_cname = new QueueStream(answer.Data))
                            {
                                var reader_cname = new BinaryReader(ms_cname);
                                var cname_buf = Read_data(reader, ms_cname).ToArray();
                                answer.CNAME = Encoding.UTF8.GetString(cname_buf, 0, cname_buf.Length);
                            }
                            break;
                    }
                }
                while (AuthoritativeNameservers.Count < AuthorityRRs)
                {
                    var authoritativeNameserver = new DnsAuthoritativeNameserver();
                    Read_DnsAnswer(authoritativeNameserver, reader);
                    AuthoritativeNameservers.Add(authoritativeNameserver);

                    if (authoritativeNameserver.DataLength > 0)
                    {
                        using (var ms_auth = new QueueStream(authoritativeNameserver.Data))
                        {
                            var nbuf = Read_data(reader, ms_auth).ToArray();
                            authoritativeNameserver.PrimaryNameserver = Encoding.UTF8.GetString(nbuf, 0, nbuf.Length);
                            if (!ms_auth.CanRead) continue;
                            nbuf = Read_data(reader, ms_auth).ToArray();
                            authoritativeNameserver.ResponsibleAuthorityMailbox = Encoding.UTF8.GetString(nbuf, 0, nbuf.Length);
                            if (!ms_auth.CanRead) continue;
                            authoritativeNameserver.SerialNumber = ms_auth.DequeueInt64_32();
                            if (!ms_auth.CanRead) continue;
                            authoritativeNameserver.RefreshInterval = ms_auth.DequeueInt32_Reversed();
                            if (!ms_auth.CanRead) continue;
                            authoritativeNameserver.RetryInterval = ms_auth.DequeueInt32_Reversed();
                            if (!ms_auth.CanRead) continue;
                            authoritativeNameserver.ExpireLimit = ms_auth.DequeueInt32_Reversed();
                            if (!ms_auth.CanRead) continue;
                            authoritativeNameserver.MinimumTTL = ms_auth.DequeueInt32_Reversed();
                        }
                    }
                }
                while (Additionals.Count < AdditionalRRs)
                {
                    var additional = new DnsAdditional();
                    Read_DnsAnswer(additional, reader);
                    Additionals.Add(additional);
                }
            }
        }
        private void Read_DnsAnswer(DnsDataResult data, BinaryReader reader)
        {
            Read_DnsQuestion(data, reader);
            data.LiveTime = Read_int(reader);
            data.DataLength = Read_ushort(reader);
            data.Data = reader.ReadBytes(data.DataLength);
        }
        private void Read_DnsQuestion(DnsQuestion question, BinaryReader reader)
        {
            var nbuf = Read_c0(reader).ToArray();
            question.Name = Encoding.UTF8.GetString(nbuf, 0, nbuf.Length);
            question.Type = Read_ushort(reader);
            question.Class = Read_ushort(reader);
        }

        private ushort Read_ushort(BinaryReader reader)
        {
            var rlt = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0);
            return rlt;
        }
        private int Read_int(BinaryReader reader)
        {
            var rlt = BitConverter.ToInt32(reader.ReadBytes(4).Reverse().ToArray(), 0);
            return rlt;
        }
        private IList<byte> Read_data(BinaryReader reader, QueueStream data_reader)
        {
            byte len;
            var lst = new List<byte>();

            do
            {
                len = data_reader.DequeueByte();

                if (len == 0xc0)
                {
                    var c0_ix = data_reader.DequeueByte();
                    var ix = reader.BaseStream.Position;
                    reader.BaseStream.Position = c0_ix;
                    lst.AddRange(Read_c0(reader));
                    reader.BaseStream.Position = ix;
                    break;
                }
                else if (len == 0x00)
                {
                    lst.RemoveAt(lst.Count - 1);
                    break;
                }
                else
                {
                    lst.AddRange(data_reader.Dequeue(len));
                    lst.Add((byte)'.');
                }
            } while (data_reader.CanRead);
            return lst;
        }
        private IList<byte> Read_c0(BinaryReader reader)
        {
            byte len;
            var lst = new List<byte>();

            do
            {
                len = reader.ReadByte();

                if (len == 0xc0)
                {
                    var c0_ix = reader.ReadByte();
                    var ix = reader.BaseStream.Position;
                    reader.BaseStream.Position = c0_ix;
                    lst.AddRange(Read_c0(reader));
                    reader.BaseStream.Position = ix;
                    break;
                }
                else if (len == 0x00)
                {
                    lst.RemoveAt(lst.Count - 1);
                    break;
                }
                else
                {
                    lst.AddRange(reader.ReadBytes(len));
                    lst.Add((byte)'.');
                }
            } while (reader.BaseStream.CanRead);
            return lst;
        }
        public byte[] GetData()
        {
            using (var ms = new MemoryStream())
            {
                WriteBytes(ms, TransactionID);
                WriteBytes(ms, Flags);
                WriteBytes(ms, (ushort)Queries.Count);
                WriteBytes(ms, AnswerRRs);
                WriteBytes(ms, AuthorityRRs);
                WriteBytes(ms, AdditionalRRs);
                var c0_dict = new Dictionary<string, byte>(); //c0 可引用区域
                if (Queries != null)
                foreach (var q in Queries)
                {
                    WriteQuestion(ms, q, c0_dict);
                }
                if (Answers!= null)
                foreach (var answer in Answers)
                {
                    WriteQuestion(ms, answer, c0_dict);
                    if (answer.Address != null)
                    {
                        answer.Data = answer.Address.GetAddressBytes();
                        answer.DataLength = (ushort)answer.Data.Length;
                    }
                    if (answer.Type == Type_CNAME)
                    {
                        var ix_cname = (int)ms.Position + 6;
                        using(var ms_cname =  new MemoryStream())
                        {
                            WriteString(answer.CNAME, ms_cname, c0_dict, ix_cname);
                            answer.Data = ms_cname.ToArray();
                        }
                        answer.DataLength = (ushort)answer.Data.Length;
                    }
                    WriteData(ms, answer, c0_dict);
                }
                if (AuthoritativeNameservers != null)
                foreach (var authoritativeNameserver in AuthoritativeNameservers)
                {
                    WriteQuestion(ms, authoritativeNameserver, c0_dict);
                    var ix_data = (int) ms.Position + 6;
                    using (var ms_auth = new MemoryStream())
                    {
                        if (authoritativeNameserver.PrimaryNameserver != null)
                        {
                            WriteString(authoritativeNameserver.PrimaryNameserver, ms_auth, c0_dict, ix_data);
                            if (authoritativeNameserver.ResponsibleAuthorityMailbox != null)
                            {
                                WriteString(authoritativeNameserver.ResponsibleAuthorityMailbox, ms_auth, c0_dict, ix_data);
                                WriteBytes(ms_auth, authoritativeNameserver.SerialNumber);
                                WriteBytes(ms_auth, authoritativeNameserver.RefreshInterval);
                                WriteBytes(ms_auth, authoritativeNameserver.RetryInterval);
                                WriteBytes(ms_auth, authoritativeNameserver.ExpireLimit);
                                WriteBytes(ms_auth, authoritativeNameserver.MinimumTTL);
                            }
                        }
                        authoritativeNameserver.Data = ms_auth.ToArray();
                        authoritativeNameserver.DataLength = (ushort)authoritativeNameserver.Data.Length;
                    }
                    WriteData(ms, authoritativeNameserver, c0_dict);
                }
                if (Additionals != null)
                foreach (var additional in Additionals)
                {
                    WriteQuestion(ms, additional, c0_dict);
                    if (additional.Address != null)
                        {
                        additional.Data = additional.Address.GetAddressBytes();
                        additional.DataLength = (ushort)additional.Data.Length;
                    }
                    WriteData(ms, additional, c0_dict);
                }
                return ms.ToArray();
            }
        }

        private void WriteData(MemoryStream ms, DnsDataResult data, IDictionary<string, byte> c0_dict)
        {
            WriteBytes(ms, data.LiveTime);
            WriteBytes(ms, data.DataLength);
            ms.Write(data.Data, 0, data.DataLength);
        }

        private void WriteQuestion(MemoryStream ms, DnsQuestion question, IDictionary<string, byte> c0_dict)
        {
            question.Name_punycode = Punycode.Encode(question.Name);
            WriteString(question.Name_punycode, ms, c0_dict);

            WriteBytes(ms, question.Type);
            WriteBytes(ms, question.Class);
        }

        private void WriteString(string data, MemoryStream ms, IDictionary<string, byte> c0_dict, int flag= 0)
        {
            var ix = -1;
            var name = data;
            do
            {
                if (c0_dict.ContainsKey(name))
                {
                    ms.WriteByte(0xc0);
                    ms.WriteByte(c0_dict[name]);
                    ix = -2;
                    break;
                }
                c0_dict[name] = (byte)(ms.Position + flag);
                ix = name.IndexOf('.');
                var _name = name;
                if (ix != -1)
                {
                    _name = name.Substring(0, ix);
                    name = name.Substring(ix + 1);
                }
                var buf = Encoding.UTF8.GetBytes(_name);
                ms.WriteByte((byte)_name.Length);
                ms.Write(buf, 0, buf.Length);

            } while (ix != -1);
            if (ix == -1)
            {
                ms.WriteByte(0x00);
            }
        }
        private void WriteBytes(MemoryStream ms, ushort data)
        {
            var _data = BitConverter.GetBytes(data);
            ms.WriteByte(_data[1]);
            ms.WriteByte(_data[0]);
        }

        private void WriteBytes(MemoryStream ms, int data)
        {
            var _data = BitConverter.GetBytes(data);
            ms.WriteByte(_data[3]);
            ms.WriteByte(_data[2]);
            ms.WriteByte(_data[1]);
            ms.WriteByte(_data[0]);
        }
        private void WriteBytes(MemoryStream ms, long data)
        {
            var _data = BitConverter.GetBytes(data);
            ms.WriteByte(_data[3]);
            ms.WriteByte(_data[2]);
            ms.WriteByte(_data[1]);
            ms.WriteByte(_data[0]);
        }
    }
}
