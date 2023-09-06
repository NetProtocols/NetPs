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
    public class QueueStream : MemoryStream, IDisposable, IQueueStream
    {
        public const int MAX_STACK_LENGTH = 10240; //80K，可扩大
        // 读取点s
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

        private bool locked { get; set; } = false;
        private byte[] x_buffer { get; set; }

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
            this.x_buffer = bytes;
            nLength = bytes.Length;
        }

        /// <summary>
        /// Gets a value indicating whether gets 队列为空.
        /// </summary>
        public virtual bool IsEmpty => this.nLength == 0;

        /// <inheritdoc/>
        public override long Length => this.nLength;

        public override bool CanRead => this.Length != 0 && base.CanRead;
        public override bool CanWrite => base.CanWrite;
        public virtual bool is_disposed { get; private set; } = false;
        public virtual bool IsClosed { get; private set; } = false;
        public virtual long WritePosition => this.w + 1;
        public virtual long ReadPosition => this.r + 1;
        public virtual byte[] Buffer
        { 
            get
            {
                if (this.x_buffer != null) return x_buffer;
                return base.GetBuffer(); 
            }
        }
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
        private long get_record_in_len(long length)
        {
            var len = length;
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

            return len;
        }
        private void record_in(long len)
        {
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
        }

        private void ok_length(long len)
        {
            if (base.Length < len)
            {
                //扩大
                base.SetLength(len);
                base.Flush();
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
            try
            {
                lock (this)
                {
                    if (this.is_disposed || block.Length == 0 || length == 0 || !base.CanWrite)
                    {
                        return;
                    }
                    else if (length > block.Length || length < 0)
                    {
                        length = block.Length;
                    }
                    var len = get_record_in_len(length);
                    {
                        //this.SafePositionSet(this.w + 1);
                        this.ok_length(this.WritePosition + len);
                        Array.Copy(block, offset, this.Buffer, (int)this.WritePosition, (int)len);
                    }
                    record_in(len);
                    if (length > len)
                    {
                        this.Enqueue(block, (int)len, (int)(length - len));
                    }
                    //Flush减少内存回收
                    //this.Flush();
                }
            }
            catch (UnauthorizedAccessException)
            {
                if (!this.is_disposed) throw;
            }
        }

        public virtual int RequestRead(int length) => (int)this.get_record_out_len(length);
        public virtual void RecordRead(int length) => this.record_out(length);

        public virtual void CopyTo(QueueStream stream, int length)
        {
            try
            {
                long len = length;
                lock (this)
                {
                    if (length == 0 || stream.is_disposed || this.is_disposed || this.IsEmpty || !base.CanRead || !stream.CanWrite) return;
                    // 限制最大读取
                    if (length < 0 || length > this.nLength) length = (int)this.nLength;

                    lock (stream)
                    {
                        len = this.get_record_in_len(length);
                        var lenb = stream.get_record_out_len(length);
                        // 如果可读取的长度, 不满足单次可写
                        if (len < lenb) { len = lenb; }
                        {
                            stream.ok_length(stream.WritePosition + len);
                            Array.Copy(this.Buffer, (int)this.ReadPosition, stream.Buffer, (int)stream.WritePosition, (int)len);
                        }
                        record_out(len);
                        stream.record_in(len);
                    }
                }
                if (length > len)
                {
                    this.CopyTo(stream, (int)(length - len));
                }
            }
            catch (UnauthorizedAccessException)
            { 
                if (!this.is_disposed) throw;
            }
        }

        private long get_record_out_len(long length)
        {
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
            return len;
        }

        private void record_out(long len)
        {
            this.nLength -= len;
            this.r += len;
        }

        /// <summary>
        /// 放出队列.
        /// </summary>
        /// <param name="block">数据块.</param>
        /// <param name="offset">偏移.</param>
        /// <param name="length">长度.</param>
        public virtual int Dequeue(byte[] block, int offset = 0, int length = -1)
        {
            try
            {
                lock (this)
                {
                    if (this.is_disposed || block.Length == 0 || length == 0 || this.IsEmpty || !base.CanRead)
                    {
                        return 0;
                    }
                    else if (length > block.Length || length < 0)
                    {
                        length = block.Length;
                    }

                    var len = get_record_out_len(length);
                    {
                        //this.SafePositionSet(this.r + 1);
                        Array.Copy(this.Buffer, (int)this.ReadPosition, block, offset, (int)len);
                    }
                    record_out(len);

                    if (length > len)
                    {
                        len += this.Dequeue(block, (int)len, (int)(length - len));
                    }

                    if (this.IsEmpty)
                    {
                        //this.Clear();
                        this.x = this.r = this.w = this.enda = this.endb = -1;
                        //this.SetLength(0);
                    }

                    return (int)len;
                }
            }
            catch (UnauthorizedAccessException)
            {
                if (!this.is_disposed) throw;
            }
            return 0;
        }

        /// <summary>
        /// 放出队列.
        /// </summary>
        /// <remarks>可能导致内存泄露</remarks>
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
                this.Dequeue(outbuf);
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
            this.Dequeue(bytes, 0, 4);
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
            return this.Dequeue(buffer, offset, count);
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
            var buffer = new byte[4096];
            while (this.CanRead)
            {
                var len = this.Dequeue(buffer, 0, 4096);
                stream.Write(buffer, 0, len);
            }
        }

        public override void Close()
        {
            this.IsClosed = true;
            base.Close();
        }

        /// <summary>
        /// 锁定
        /// </summary>
        public void LOCK()
        {
            lock (this)
            {
                if (this.locked) return;
                this.locked = true;
            }
            this.Position = 0;
            this.Clear();
            this.Flush();
        }

        /// <summary>
        /// 解锁
        /// </summary>
        public void UNLOCK()
        {
            lock (this)
            {
                if (!this.locked) return;
                this.locked = false;
            }
        }

        public new void Dispose()
        {
            lock (this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
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
            if (!this.IsClosed && !this.locked) base.Write(buffer, offset, count);
        }

        /// <summary>
        /// 安全的读取
        /// </summary>
        private int SafeRead(byte[] buffer, int offset, int count)
        {
            //fix: ObjectDisposedException Cannot access a closed Stream
            if (!this.IsClosed && !this.locked) return base.Read(buffer, offset, count);
            return 0;
        }

        /// <summary>
        /// 安全的指针位置设置
        /// </summary>
        /// <param name="position"></param>
        private void SafePositionSet(long position)
        {
            if (!this.IsClosed && !this.locked) this.Position = position;
        }
    }
}
