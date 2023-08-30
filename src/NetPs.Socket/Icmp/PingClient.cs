namespace NetPs.Socket.Icmp
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    public delegate void PingReceivedHandle(IPingPacket packet);
    public class PingClient : SocketCore, IDisposable
    {
        protected bool v6 = false;
        public int Timeout { get; private set; }
        private IReceivedFrom ReceivedFrom { get; set; }
        public int BufferSize { get; private set; }
        public PingClient(int timeout, int buffer_size)
        {
            BufferSize = buffer_size;
            receive_buffer = new byte[BufferSize];
            SetTimeout(timeout);
            BindTo();
            this.OnPingReceivedObservable = Observable.FromEvent<PingReceivedHandle, IPingPacket>(handler => packet => handler(packet), evt => this.OnPingReceived += evt, evt => this.OnPingReceived -= evt);
        }

        public event PingReceivedHandle OnPingReceived;
        public virtual IObservable<IPingPacket> OnPingReceivedObservable { get; private set; }

        public override void Dispose()
        {
            base.Dispose();
        }
        public virtual void SetTimeout(int timeout)
        {
            Timeout = timeout;
        }

        protected virtual void BindTo()
        {
            Socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, Timeout);
            this.Socket.Bind(new IPEndPoint(IPAddress.Any, 0));
        }

        public virtual async Task<IPingPacket> Ping(IPingPacket packet)
        {
            var p = new IPEndPoint(packet.Address, 0);
            if (!receiving) StartReceive();

            var rep = this.OnPingReceivedObservable
                    .Timeout(TimeSpan.FromMilliseconds(this.Timeout))
                    .FirstAsync();
            var task = rep.GetAwaiter();
            StartSend(packet.GET(), p);
            var pkg = await task;
            return pkg;
        }

        public virtual void StartSend(byte[] data, EndPoint endPoint, int offset = 0, int length = -1)
        {
            lock (this)
            {
                if (IsDisposed) return;
            }
            if (length <= 0)
            {
                length = data.Length;
            }

            send_to(data, offset, length, endPoint);
        }


        private void send_to(byte[] data, int offset, int length, EndPoint endPoint)
        {
            Socket.SendTo(data, offset, length, SocketFlags.None, endPoint);
        }


        private EndPoint endPoint;
        private byte[] receive_buffer;
        private int n_received = 0;
        private bool receiving = false;
        public bool Receiving => receiving;

        public virtual void StartReceive()
        {
            lock (this)
            {
                if (receiving || IsDisposed) return;
                receiving = true;
            }
            receive_from();
        }
        private void receive_from()
        {
            try
            {
                lock (this)
                {
                    if (IsDisposed) return;
                }
                if (v6) endPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
                else endPoint = new IPEndPoint(IPAddress.Any, 0);
                Socket.BeginReceiveFrom(receive_buffer, 0, receive_buffer.Length, SocketFlags.None, ref endPoint, receive_from_callback, null);
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException)
            {
                //忽略
                //receive_from();
                return;
            }
            receiving = false;
        }

        private void receive_from_callback(IAsyncResult asyncResult)
        {
            try
            {
                lock (this)
                {
                    if (IsDisposed) return;
                }
                n_received = Socket.EndReceiveFrom(asyncResult, ref endPoint);
                received();
                return;
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (SocketException)
            {
                //忽略
                //receive_from();
                return;
            }
            receiving = false;
        }

        private void received()
        {
            if (n_received <= 0) return;
            var buffer = new byte[n_received];
            Array.Copy(receive_buffer, buffer, n_received);
            if (ReceivedFrom != null) ReceivedFrom.Received(buffer, endPoint as IPEndPoint);
            if (OnPingReceived != null) OnPingReceived.Invoke(new PingPacket(buffer) { Address = (endPoint as IPEndPoint)?.Address });
            //receive_from();
        }

        protected override void OnConnected()
        {
        }

        protected override void OnClosed()
        {
        }
    }
}
