namespace NetPs.Tcp
{
    using NetPs.Socket;
    using System;
    public class TcpQueueTx : TcpTx
    {
        private bool is_disposed = false;
        private IQueueStream cache { get; set; }
        public TcpQueueTx() : base()
        {
            this.cache = SocketCore.StreamPool.GET();
        }

        /// <summary>
        /// Gets 发送队列.
        /// </summary>
        public virtual IQueueStream TransportCache => this.cache;

        public override void Dispose()
        {
            lock (this)
            {
                if (is_disposed) return;
                is_disposed = true;
            }
            base.Dispose();
            if (this.cache != null && this.cache is QueueStream queue)
            {
                SocketCore.StreamPool.PUT(queue);
                this.cache = null;
            }
        }

        public override void Transport(byte[] data, int offset = 0, int length = -1)
        {
            this.cache.Enqueue(data, offset, length);

            if (base.to_start())
            {
                transport_next();
            }
        }

        protected override void OnTransported()
        {
            if (this.cache.IsEmpty)
            {
                base.OnTransported();
            }
            else
            {
                this.OnBufferTransported();
            }
        }

        protected virtual void OnBufferTransported()
        {
            transport_next();
        }

        private void transport_next()
        {
            var length = this.cache.RequestRead(this.TransportBufferSize);
            base.Transport(this.cache.Buffer, (int)this.cache.ReadPosition, length);
        }
    }
}