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
        public static string ToHexString(this byte[] data)
        {
            return data.ToHexString(0, data.Length);
        }

        internal static void CopyFrom(this byte[] data, uint num, int offset)
        {
            Array.Copy(BitConverter.GetBytes(num), 0, data, offset, sizeof(uint));
        }
        internal static void CopyFrom_Reverse(this byte[] data, uint num, int offset)
        {
            data[offset] = (byte)((num >> 24) & 0xff);
            data[offset + 1] = (byte)((num >> 16) & 0xff);
            data[offset + 2] = (byte)((num >> 8) & 0xff);
            data[offset + 3] = (byte)((num) & 0xff);
        }
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
