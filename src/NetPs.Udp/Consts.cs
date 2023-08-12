using System;
using System.Collections.Generic;
using System.Text;

namespace NetPs.Udp
{
    public struct Consts
    {
        public const int BUFFER_SIZE = 15616;
        //接收缓冲区大小
        public static int ReceiveBytes = BUFFER_SIZE;
        //发送缓冲区大小
        public static int TransportBytes = BUFFER_SIZE;
        public static int SocketPollTime = 3600;
    }
}
