## 简介

NetPs基类，对原始的System.Net.Socket类进行简单封装。

其中为队列byte流提供类QueueStream类，实现了Enqueue\Dequeue。

ISocketUri增加了任意地址的解析。

该库支持提前预热来加快首次运行速度，可以通过调用方法进行使用 `NetPs.Socket.Eggs.Food.Heating();`

## How To Use?

- **01. SocketCore**
  
```csharp
class Host : SocketCore
{
//...
}
```

- **02. IQueueStream**
  
```csharp
var queue = new QueueStream();

queue.Enqueue(new byte[] { 1,2,3});
var buf1 = queue.Dequeue(1); // [1]
var buf2 = queue.Dequeue(2); // [2,3]
```
- **03. InsideSocketUri / SocketUri**
(defualt use InsideSocketUri)
```csharp
var uri1 = new SocketUri("[::]:1025");//IPv6Any
var uri2 = new SocketUri("0.0.0.0:1025");//Any
var uri3 = new SocketUri("[::1]:1025");//IPv6Loopback
var uri4 = new SocketUri("127.0.0.1:1025");//Loopback
```
!SocketUri 基于System.Uri，Linux下有可能发生```ICU package not installed```.

- **04. PingClient / PingV6Client**

```
var client = new PingClient(3000, 8192); // max wait 3 second; receive buffer 8192 byte;
var packet = new PingPacket(PingPacket.DATA_32, PingPacketKind.Request)
{
    Address = InsideSocketUri.ParseIPAddress("127.0.0.1")
};
var rlt = await client.Ping(packet);
// ipv6
var v6client = new PingV6Client(3000, 8192); // max wait 3 second; receive buffer 8192 byte;
packet.Address = InsideSocketUri.ParseIPAddress("::1");
var rlt = await v6client.Ping(packet);
```

*<u>NetPs以实现现有网络协议库为目标，为数据交互提供基础支撑。</u>*