namespace NetPs.Socket.Memory
{
    using System;

    /// <summary>
    /// 循环数组栈
    /// </summary>
    /// <remarks>
    /// 数据遵守 先进先出
    /// </remarks>
    public struct LoopArray <T>
    {
        internal T[] array { get; set; }
        internal int write_pos { get; set; }
        internal int length { get; set; }
        public static LoopArray<T> New(int length)
        {
            var arr = new LoopArray<T>();
            arr.array = new T[length];
            arr.length = length;
            arr.write_pos = 0;
            return arr;
        }
        public T Get(int index)
        {
            index += write_pos;
            if (index < length)
            {
                return array[index];
            }
            else
            {
                return array[index - length];
            }
        }
        /// <summary>
        /// 复制当前数据到 指定数组
        /// </summary>
        /// <param name="start">起始索引</param>
        /// <param name="dst">目标数组</param>
        /// <param name="offset">目标写入索引</param>
        /// <param name="length">写入次数</param>
        public void CopyTo(int start, T[] dst, int offset, int length)
        {
            start += write_pos;
            if (start >= this.length)
            {
                start = start - this.length;
            }
            if (length + start > this.length)
            {
                Array.Copy(array, start, dst, offset, this.length - start);
                Array.Copy(array, 0, dst, offset + this.length - start, length - this.length + start);
            }
            else
            {
                Array.Copy(array, start, dst, offset, length);
            }
        }
        public void Push(T value)
        {
            array[write_pos] = value;
            this.write_pos ++;
            if (write_pos >= length)
            {
                write_pos = 0;
            }
        }
        /// <summary>
        /// 反复添加一个数据指定字数
        /// </summary>
        /// <param name="value">数据</param>
        /// <param name="times">次数</param>
        public void Push(T value, int times)
        {
            if (times + write_pos > this.length)
            {
                for (var i = 0; i < this.length - write_pos; i++)
                {
                    array[write_pos + i] = value;
                }
                for (var i = 0; i < times + write_pos - this.length; i++)
                {
                    array[i] = value;
                }
                write_pos = times + write_pos - this.length;
            }
            else
            {
                for (var i =0; i<times; i++)
                {
                    array[write_pos + i] = value;
                }
                this.write_pos += times;
                if (write_pos == this.length) write_pos = 0;
            }
        }
        /// <summary>
        /// 从数组内得到数据，添加到该实例
        /// </summary>
        /// <param name="buffer">数据源</param>
        /// <param name="offset">起始索引</param>
        /// <param name="length">读取次数</param>
        public void Push(T[] buffer, int offset, int length)
        {
            if (length + write_pos > this.length)
            {
                Array.Copy(buffer, offset, array, write_pos, this.length - write_pos);
                Array.Copy(buffer, offset + this.length - write_pos, array, 0 , length + write_pos - this.length );
                write_pos = length  + write_pos - this.length;
            }
            else
            {
                Array.Copy(buffer, offset, array, this.write_pos, length);
                this.write_pos += length;
                if (write_pos == this.length) write_pos = 0;
            }
        }
    }
}
