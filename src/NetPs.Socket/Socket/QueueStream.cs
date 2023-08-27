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
    public class QueueStream : MemoryStream, IDisposable
    {
        public const int MAX_STACK_LENGTH = 4096;
        // 读取点
        private long r { get; set; }

        // 当前写入点
        private long w { get; set; }

        // 转折点
        private long x { get; set; }

        // 前结尾
        private long enda { get; set; }

        // 末结尾
        private long endb { get; set; }

        private long nLength { get; set; }

        private object locker { get; set; } = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueStream"/> class.
        /// </summary>
        public QueueStream() : base(0)
        {
            this.Clear();
        }

        public QueueStream(int capacity) : base(capacity)
        {
            this.Clear();
        }

        /// <summary>
        /// 只读
        /// </summary>
        /// <param name="bytes"></param>
        public QueueStream(byte[] bytes) : base(bytes)
        {
            this.Clear();
            nLength = bytes.Length;
        }

        /// <summary>
        /// Gets a value indicating whether gets 队列为空.
        /// </summary>
        public virtual bool IsEmpty => this.nLength == 0;

        /// <inheritdoc/>
        public override long Length => this.nLength;
        
        public override bool CanRead => this.Length != 0;
        public virtual bool IsDisposed { get; private set; } = false;
        public virtual bool IsClosed { get; private set; } = false;

        /// <summary>
        /// 清空队列.
        /// </summary>
        public virtual void Clear()
        {
            lock (this)
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
            lock (this)
            {
                try
                {
                    if (length + this.Length > Capacity)
                    {
                        Capacity += length;
                    }
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

                    this.SafePositionSet(this.w + 1);
                    for (long i = offset; !IsDisposed && i < len; i += MAX_STACK_LENGTH)
                    {
                        //防止占堆溢出: memorystream.write方法使用递归调用
                        if (i + MAX_STACK_LENGTH < len) this.SafeWrite(block, (int)i, MAX_STACK_LENGTH);
                        else this.SafeWrite(block, (int)i, (int)(len % MAX_STACK_LENGTH));
                    }
                    //base.Write(block, offset, (int)len);
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
                catch (ObjectDisposedException)
                {
                    //不需要进行处理实例已经Closed, 释放了
                }
            }
        }

        /// <summary>
        /// 放出队列.
        /// </summary>
        /// <param name="block">数据块.</param>
        /// <param name="offset">偏移.</param>
        /// <param name="length">长度.</param>
        public virtual int Dequeue(ref byte[] block, int offset = 0, int length = -1)
        {
            if (block == null || block.Length == 0 || length == 0 || this.IsEmpty)
            {
                return 0;
            }
            else if (length > block.Length || length < 0)
            {
                length = block.Length;
            }

            long len = length;
            lock (this)
            {
                try
                {
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

                    this.SafePositionSet(this.r + 1);
                    var rlt = 0;
                    for (long i = offset; !IsDisposed && i < len; i += MAX_STACK_LENGTH)
                    {
                        //防止占堆溢出: memorystream.read方法使用递归调用
                        if (i + MAX_STACK_LENGTH < len) rlt += this.SafeRead(block, (int)i, MAX_STACK_LENGTH);
                        else rlt += this.SafeRead(block, (int)i, (int)(len % MAX_STACK_LENGTH));
                    }
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

                    return rlt;
                }
                catch(ObjectDisposedException)
                {
                    //不需要进行处理实例已经Closed, 释放了
                    return 0;
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
            lock (this)
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

        public virtual byte DequeueByte()
        {
            return this.Dequeue(1)[0];
        }

        /// <summary>
        /// 出 Int32.
        /// </summary>
        /// <returns>整形.</returns>
        public virtual int DequeueInt32()
        {
            return BitConverter.ToInt32(this.Dequeue(4), 0);
        }

        /// <summary>
        /// 出 Int32.
        /// </summary>
        /// <returns>整形.</returns>
        public virtual Int64 DequeueInt64_32()
        {
            byte[] bytes = new byte[8];
            this.Dequeue(ref bytes, 0, 4);
            bytes[4] = bytes[0];
            bytes[0] = bytes[3];
            bytes[3] = bytes[4]; ;
            bytes[4] = bytes[1];
            bytes[1] = bytes[2];
            bytes[2] = bytes[4];
            bytes[4] = 0;
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// 存入 Int32.
        /// </summary>
        /// <param name="data">整形.</param>
        public virtual void EnqueueInt32(int data)
        {
            this.Enqueue(BitConverter.GetBytes(data));
        }

        public virtual int DequeueInt32_Reversed()
        {
            byte[] bytes;
            bytes = this.Dequeue(4);
            var buf = bytes[0];
            bytes[0] = bytes[3];
            bytes[3] = buf;
            buf = bytes[1];
            bytes[1] = bytes[2];
            bytes[2] = buf;
            return BitConverter.ToInt32(bytes, 0);
        }

        public override int ReadByte()
        {
            return this.DequeueByte();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.Dequeue(ref buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.Enqueue(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            this.Enqueue(new byte[] { value }, 0, 1);
        }

        public override void WriteTo(Stream stream)
        {
            var length = (int)this.Length;
            var buffer = new byte[4096];
            while (this.CanRead)
            {
                var len = this.Dequeue(ref buffer, 0, 4096);
                stream.Write(buffer, 0, len);
            }
        }

        public override void Close()
        {
            this.IsClosed = true;
            base.Close();
        }
        void IDisposable.Dispose()
        {
            this.IsDisposed = true;
            base.Dispose();
        }

        private bool is_main(long point) => this.x == -1 || this.x < point;

        private bool before_is_empty() => this.x == -1 && this.enda == -1 && this.nLength > 0 && this.endb == this.r;

        private long before_empty() => this.x - this.r - this.enda;

        /// <summary>
        /// 安全的写入
        /// </summary>
        private void SafeWrite(byte[] buffer, int offset, int count)
        {
            //fix: ObjectDisposedException Cannot access a closed Stream
            if (!this.IsClosed) base.Write(buffer, offset, count);
        }

        /// <summary>
        /// 安全的读取
        /// </summary>
        private int SafeRead(byte[] buffer, int offset, int count)
        {
            //fix: ObjectDisposedException Cannot access a closed Stream
            if (!this.IsClosed) return base.Read(buffer, offset, count);
            return 0;
        }

        /// <summary>
        /// 安全的指针位置设置
        /// </summary>
        /// <param name="position"></param>
        private void SafePositionSet(long position)
        {
            if (!this.IsClosed) this.Position = position;
        }
    }
}
