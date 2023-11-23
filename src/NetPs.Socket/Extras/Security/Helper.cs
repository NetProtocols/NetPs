namespace NetPs.Socket.Extras.Security
{
    using System;
    using System.Text;

    public static class Helper
    {
        public static string ToHexString(this byte[] data, int offset, int length)
        {
            var text = new StringBuilder();
            for (var i = offset; i < length; i++)
            {
                if (data[i] < 0x10) text.Append('0');
                text.Append(Convert.ToString(data[i], 16));
            }

            return text.ToString();
        }
        /// <summary>
        /// 十六进制转为字符串显示格式
        /// </summary>
        public static string ToHexString(this byte[] data)
        {
            return data.ToHexString(0, data.Length);
        }

        /// <summary>
        /// 将uint 转换为 4 byte并填充到数组中
        /// </summary>
        internal static void CopyFrom(this byte[] data, uint num, int offset, byte count = sizeof(uint))
        {
            Array.Copy(BitConverter.GetBytes(num), 0, data, offset, count);
        }
        internal static void CopyFrom(this byte[] data, ulong num, int offset, byte count = sizeof(ulong))
        {
            Array.Copy(BitConverter.GetBytes(num), 0, data, offset, count);
        }
        /// <summary>
        /// 将uint 转换为 4byte 从后向前填充到数组中
        /// </summary>
        internal static void CopyFrom_Reverse(this byte[] data, uint num, int offset, byte count = sizeof(uint))
        {
            for (byte i = 0; i< count; i++)
            {
                data[offset + i] = (byte)(num >> ((i ^ 3)<< 3));
            }
        }
        internal static void CopyFrom_Reverse(this byte[] data, ulong num, int offset, byte count = sizeof(ulong))
        {
            for (byte i = 0; i< count; i++)
            {
                data[offset + i] = (byte)(num >> ((i ^ 7) << 3));
            }
        }
        internal static void CopyFrom_Reverse2(this byte[] data, ulong num, int offset, byte count = sizeof(ulong))
        {
            for (byte i = 0; i < count; i++)
            {
                data[offset + i] = (byte)(num >> (((11 - i)& 7) << 3));
            }
        }
        internal static void CopyFrom(this byte[] data, int data_offset, ulong[][] array, int offset, uint length, uint array_length = 5)
        {
            int i, j = offset;
            for (i = data_offset; i < length >> 3; i++, j += 8)
            {
                data.CopyFrom(array[i / array_length][i % array_length], j);
            }
            if ((length & 0b111) != 0)
            {
                data.CopyFrom(array[i / array_length][i % array_length], j, (byte)(length & 0b111));
            }
        }
        /// <summary>
        /// 将uint[] 填充到byte[] 中
        /// </summary>
        internal static void CopyFrom(this byte[] data, int data_offset, uint[] nums, int offset, int length)
        {
            for (int i = data_offset, j = 0; j <length; i+= sizeof(uint), j++)
            {
                Array.Copy(BitConverter.GetBytes(nums[offset + j]), 0, data, i, sizeof(uint));
            }
        }
        internal static void CopyFrom_Reverse(this byte[] data, int data_offset, uint[] nums, int offset, int length)
        {
            uint i, j;
            byte k;
            for (i = (uint)data_offset, j = 0; j < length; i += 4, j++)
            {
                for (k = 0; k < 4; k++) data[i + k] = (byte)(nums[offset + j] >> ((k ^ 3) << 3));
            }
        }
        //反转bit
        internal static byte Bitrev(byte x)
        {
            byte i;
            byte tmp;
            for (i = 0, tmp = 0; i < 8; i++, x >>= 1) if ((x & 1) == 1) tmp |= (byte)(1 << (i ^ 7));
            return tmp;
        }
        internal static ushort Bitrev(ushort x)
        {
            byte i;
            ushort tmp;
            for (i = 0, tmp = 0; i < 16; i++, x >>= 1) if ((x & 1) == 1) tmp |= (ushort)(1 << (i ^ 15));
            return tmp;
        }
        internal static ulong Bitrev(ulong x)
        {
            byte i;
            ulong tmp;
            for (i = 0, tmp = 0; i < 64; i++, x >>= 1) if ((x & 1) == 1) tmp |= (ulong)1 << (i ^ 63);
            return tmp;
        }
        internal static uint Bitrev(uint x)
        {
            byte i;
            uint tmp;
            for (i = 0, tmp = 0; i < 32; i++, x >>= 1) if ((x & 1) == 1) tmp |= (uint)1 << (i ^ 31);
            return tmp;
        }
    }
}
