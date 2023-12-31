﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NetPs.Tcp
{
    public struct Consts
    {
        public const int BUFFER_SIZE = 15616; //4K +24 tcp header :15616
        //接收缓冲区大小
        public static int ReceiveBytes = BUFFER_SIZE;
        //发送缓冲区大小
        public static int TransportBytes = BUFFER_SIZE;
        public static int MaxAcceptClient = 650<<20;
        public static int SocketPollTime = 3600;//500ms
    }
}
