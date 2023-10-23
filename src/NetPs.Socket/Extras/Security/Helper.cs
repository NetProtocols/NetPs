namespace NetPs.Socket.Extras.Security
{
    using NetPs.Socket.Memory;
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
        internal static void CopyFrom(this byte[] data, uint num, int offset)
        {
            Array.Copy(BitConverter.GetBytes(num), 0, data, offset, sizeof(uint));
        }
        /// <summary>
        /// 将uint 转换为 4byte 从后向前填充到数组中
        /// </summary>
        internal static void CopyFrom_Reverse(this byte[] data, uint num, int offset)
        {
            data[offset] = (byte)((num >> 24) & 0xff);
            data[offset + 1] = (byte)((num >> 16) & 0xff);
            data[offset + 2] = (byte)((num >> 8) & 0xff);
            data[offset + 3] = (byte)((num) & 0xff);
        }
        internal static void CopyFrom_Reverse(this byte[] data, ulong num, int offset)
        {
            data[offset + 0] = (byte)(((num & 0xff00000000000000) >> 56) & 0xff);
            data[offset + 1] = (byte)(((num & 0x00ff000000000000) >> 48) & 0xff);
            data[offset + 2] = (byte)(((num & 0x0000ff0000000000) >> 40) & 0xff);
            data[offset + 3] = (byte)(((num & 0x000000ff00000000) >> 32) & 0xff);
            data[offset + 4] = (byte)(((num & 0x00000000ff000000) >> 24) & 0xff);
            data[offset + 5] = (byte)(((num & 0x0000000000ff0000) >> 16) & 0xff);
            data[offset + 6] = (byte)(((num & 0x000000000000ff00) >> 8) & 0xff);
            data[offset + 7] = (byte)((num & 0x00000000000000ff) & 0xff);
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
