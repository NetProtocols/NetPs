namespace NetPs.Socket
{
    using System;
    using System.IO;
#if NET35_CF
    using Array = System.Array2;
#endif

    /// <summary>
    /// 队列 Stream.
    /// </summary>
    public class QueueStream : MemoryStream
    {
        // 读取点
        private long r;

        // 当前写入点
        private long w;

        // 转折点
        private long x;

        // 前结尾
        private long enda;

        // 末结尾
        private long endb;

        private long nLength;

        private object locker = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueStream"/> class.
        /// </summary>
        public QueueStream()
        {
            this.Clear();
        }

        /// <summary>
        /// Gets a value indicating whether gets 队列为空.
        /// </summary>
        public virtual bool IsEmpty => this.nLength == 0;

        /// <inheritdoc/>
        public override long Length => this.nLength;

        /// <summary>
        /// 清空队列.
        /// </summary>
        public virtual void Clear()
        {
            lock (this.locker)
            {
                this.r = -1;
                this.w = -1;
                this.x = -1;
                this.enda = -1;
                this.endb = -1;
                this.nLength = 0;
            }
        }

        /// <summary>
        /// 进入队列.
        /// </summary>
        /// <param name="block">数据块.</param>
        /// <param name="offset">偏移.</param>
        /// <param name="length">长度.</param>
        public virtual void Enqueue(byte[] block, int offset = 0, int length = -1)
        {
            lock (this.locker)
            {
                if (block == null || block.Length == 0 || length == 0)
                {
                    return;
                }
                else if (length > block.Length || length < 0)
                {
                    length = block.Length;
                }

                long len = length;
                if (this.before_is_empty())
                {
                    // 回去写.
                    this.w = -1;
                    this.x = this.endb;
                    len = this.before_empty();
                }
                else if (!this.is_main(this.w))
                {
                    len = this.before_empty();
                    if (len == 0)
                    {
                        len = length;
                        this.w = this.x;
                    }
                }

                if (length < len)
                {
                    len = length;
                }

                this.Position = this.w + 1;
                this.Write(block, offset, (int)len);
                this.nLength += len;
                this.w += len;
                if (this.is_main(this.w))
                {
                    this.endb = this.w;
                }
                else
                {
                    this.enda = this.w;
                }

                if (length > len)
                {
                    this.Enqueue(block, (int)len, (int)(length - len));
                }
            }
        }

        /// <summary>
        /// 放出队列.
        /// </summary>
        /// <param name="block">数据块.</param>
        /// <param name="offset">偏移.</param>
        /// <param name="length">长度.</param>
        public virtual void Dequeue(ref byte[] block, int offset = 0, int length = -1)
        {
            lock (this.locker)
            {
                if (block == null || block.Length == 0 || length == 0 || this.IsEmpty)
                {
                    return;
                }
                else if (length > block.Length || length < 0)
                {
                    length = block.Length;
                }

                long len = length;
                if (this.nLength < len)
                {
                    len = this.nLength;
                }

                if (!this.is_main(this.r))
                {
                    if (this.r < this.enda)
                    {
                        len = this.enda - this.r;
                        if (len == 0)
                        {
                            this.r = -1;
                        }
                    }

                    if (this.r > this.enda)
                    {
                        len = this.x - this.r;
                    }

                    if (this.r == this.enda)
                    {
                        this.r = this.x;
                        this.enda = this.x = -1;
                    }
                }

                if (length < len)
                {
                    len = length;
                }

                this.Position = this.r + 1;
                this.Read(block, offset, (int)len);
                this.nLength -= len;
                this.r += len;

                if (length > len)
                {
                    this.Dequeue(ref block, (int)len, (int)(length - len));
                }

                if (this.IsEmpty)
                {
                    this.x = this.r = this.w = this.enda = this.endb = -1;
                    this.SetLength(0);
                }
            }
        }

        /// <summary>
        /// 放出队列.
        /// </summary>
        /// <param name="length">长度.</param>
        /// <returns>数据块.</returns>
        public virtual byte[] Dequeue(long length)
        {
            lock (this.locker)
            {
                if (this.IsEmpty)
                {
                    return null;
                }
                else if (this.nLength < length)
                {
                    length = (int)this.nLength;
                }

                var outbuf = new byte[length];
                this.Dequeue(ref outbuf);
                return outbuf;
            }
        }

        /// <summary>
        /// 出 Int32.
        /// </summary>
        /// <returns>整形.</returns>
        public virtual int DequeueInt32()
        {
            lock (this.locker)
            {
                return BitConverter.ToInt32(this.Dequeue(4), 0);
            }
        }

        /// <summary>
        /// 存入 Int32.
        /// </summary>
        /// <param name="data">整形.</param>
        public virtual void EnqueueInt32(int data)
        {
            lock (this.locker)
            {
                this.Enqueue(BitConverter.GetBytes(data));
            }
        }

        private bool is_main(long point) => this.x == -1 || this.x < point;

        private bool before_is_empty() => this.x == -1 && this.enda == -1 && this.nLength > 0 && this.endb == this.r;

        private long before_empty() => this.x - this.r - this.enda;
    }
}
