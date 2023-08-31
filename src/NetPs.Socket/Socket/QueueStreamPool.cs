namespace NetPs.Socket
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 队列流池
    /// </summary>
    /// <remarks>
    /// 通过复用queuestream, 以缓解大量使用队列流释放后触发的大量回收 GC。
    /// </remarks>
    public class QueueStreamPool: IDisposable
    {
        public const int MIN_RELEASE_DELAY = 10000000; //最小1s
        private IList<QueueStream> resources { get; }
        //最近释放时间
        private long last_release_ticks { get; set; }
        private int max_live { get; set; }
        private bool is_disposed { get; set; }
        public bool IsDisposed => this.is_disposed;
        public long Last_Relase_Ticks => this.last_release_ticks;
        public int Max_Live => this.max_live;
        /// <summary>
        /// QueueStream池
        /// </summary>
        /// <param name="max">最大保留</param>
        public QueueStreamPool(int max)
        {
            this.is_disposed = false;
            max_live = max;
            this.resources = new List<QueueStream>();
        }

        public void SET_MAX(int max)
        {
            this.max_live = max;
        }

        public void PUT(QueueStream stream)
        {
            if (stream.IsClosed) return;
            if (this.is_disposed) stream.Dispose();
            else
            {
                lock(this) resources.Add(stream);
                stream.LOCK();
            }

            Release(max_live);
        }

        public QueueStream GET()
        {
            QueueStream stream = null;
            lock (this)
            if (resources.Count != 0)
            {
                lock(this)
                {
                    var max = resources.Max(res => res.Capacity);
                    stream = resources.Where(res => res.Capacity == max).First();
                    resources.Remove(stream);
                }
            }
            if (stream == null) stream = new QueueStream();
            else
            {
                stream.Clear();
                stream.UNLOCK();
            }
            return stream;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="save_count">保留的实例数</param>
        public void Release(int save_count)
        {
            if (save_count < 0 || this.resources.Count <= save_count) return;
            if (this.IsDisposed) return;
            var now = DateTime.Now.Ticks;
            if (now - last_release_ticks < MIN_RELEASE_DELAY) return;
            last_release_ticks = now;
            lock (this)
            {
                foreach(var res in resources.OrderBy(res => res.Capacity).Take(resources.Count - save_count))
                {
                    res.Dispose();
                    this.resources.Remove(res);
                }
            }
        }

        public void Dispose()
        {
            lock(this)
            {
                if (this.is_disposed) return;
                this.is_disposed = true;
            }
            if (resources.Count > 0)
            {
                foreach (var res in resources.AsEnumerable()) res.Dispose();
            }
        }
    }
}
