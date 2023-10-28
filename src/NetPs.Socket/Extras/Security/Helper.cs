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
                data[offset + i] = (byte)(num >> ((3 - i)<< 3));
            }
            //data[offset] = (byte)((num >> 24) & 0xff);
            //data[offset + 1] = (byte)((num >> 16) & 0xff);
            //data[offset + 2] = (byte)((num >> 8) & 0xff);
            //data[offset + 3] = (byte)((num) & 0xff);
        }
        internal static void CopyFrom_Reverse(this byte[] data, ulong num, int offset, byte count = sizeof(ulong))
        {
            for (byte i = 0; i< count; i++)
            {
                data[offset + i] = (byte)(num >> ((7 - i) << 3));
            }
            //data[offset + 0] = (byte)(((num & 0xff00000000000000) >> 56) & 0xff);
            //data[offset + 1] = (byte)(((num & 0x00ff000000000000) >> 48) & 0xff);
            //data[offset + 2] = (byte)(((num & 0x0000ff0000000000) >> 40) & 0xff);
            //data[offset + 3] = (byte)(((num & 0x000000ff00000000) >> 32) & 0xff);
            //data[offset + 4] = (byte)(((num & 0x00000000ff000000) >> 24) & 0xff);
            //data[offset + 5] = (byte)(((num & 0x0000000000ff0000) >> 16) & 0xff);
            //data[offset + 6] = (byte)(((num & 0x000000000000ff00) >> 8) & 0xff);
            //data[offset + 7] = (byte)((num & 0x00000000000000ff) & 0xff);
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
            for (int i = data_offset, j = 0; j < length; i += 4, j++)
            {
                data[i] = (byte)((nums[offset + j] >> 24) & 0xff);
                data[i + 1] = (byte)((nums[offset + j] >> 16) & 0xff);
                data[i + 2] = (byte)((nums[offset + j] >> 8) & 0xff);
                data[i + 3] = (byte)((nums[offset + j]) & 0xff);
            }
        }
    }
}
