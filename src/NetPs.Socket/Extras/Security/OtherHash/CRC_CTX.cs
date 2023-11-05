namespace NetPs.Socket.Extras.Security.OtherHash
{
    using System;
    public struct CRC8_CTX
    {
        internal byte polynomial { get; set; }
        internal byte xor { get; set; }
        internal byte crc { get; set; }
        internal byte[] crc_table { get; set; }
        internal bool reflected_in { get; set; }
        internal bool reflected_out { get; set; }
        public void SetInitValue(byte val) { crc = (byte)(0 ^ val); }
        public void SetPolynomial(byte val) { polynomial = val; }
        public void SetXor(byte val) { xor = val; }
        public void SetReflectedIn() { reflected_in = true; crc = Helper.Bitrev(crc); }
        public void SetReflectedOut() { reflected_out = true; }
    }
    public struct CRC16_CTX
    {
        internal ushort polynomial { get; set; }
        internal ushort xor { get; set; }
        internal ushort crc { get; set; }
        internal ushort[] crc_table { get; set; }
        internal bool reflected_in { get; set; }
        internal bool reflected_out { get; set; }
        public void SetInitValue(short val) { crc = (ushort)(0 ^ (ushort)val); }
        public void SetPolynomial(short val) { polynomial = (ushort)val; }
        public void SetXor(short val) { xor = (ushort)val; }
        public void SetReflectedIn() { reflected_in = true; crc = Helper.Bitrev(crc); }
        public void SetReflectedOut() { reflected_out = true; }
    }
    public struct CRC32_CTX
    {
        internal uint polynomial { get; set; }
        internal uint xor { get; set; }
        internal uint crc { get; set; }
        internal uint[] crc_table { get; set; }
        internal bool reflected_in { get; set; }
        internal bool reflected_out { get; set; }
        public void SetInitValue(int val) { crc = 0 ^ (uint)val; }
        public void SetPolynomial(int val) { polynomial = (uint)val; }
        public void SetXor(int val) { xor = (uint)val; }
        public void SetReflectedIn() { reflected_in = true; crc = Helper.Bitrev(crc); }
        public void SetReflectedOut() { reflected_out = true; }
    }
    public struct CRC64_CTX
    {
        internal ulong polynomial { get; set; }
        internal ulong xor { get; set; }
        internal ulong crc { get; set; }
        internal ulong[] crc_table { get; set; }
        internal bool reflected_in { get; set; }
        internal bool reflected_out { get; set; }
        public void SetInitValue(long val) { crc = 0 ^ (ulong)val; }
        public void SetPolynomial(long val) { polynomial = (ulong)val; }
        public void SetXor(long val) { xor = (ulong)val; }
        public void SetReflectedIn() { reflected_in = true; crc = Helper.Bitrev(crc); }
        public void SetReflectedOut() { reflected_out = true; }
    }
}
