## 简介

NetPs基类，对原始的System.Net.Socket类进行简单封装。

其中为队列byte流提供类QueueStream类，实现了Enqueue\Dequeue。

SocketUri增加了任意地址的解析。

该库支持提前预热来加快首次运行速度，可以通过调用方法进行使用 `NetPs.Socket.Eggs.Food.Heating();`

## How To Use?

- **SocketCore**
  
```csharp
class Host : SocketCore
{
//...
}
```

- **QueueStream**
  
```csharp
var queue = new QueueStream();

queue.Enqueue(new byte[] { 1,2,3});
var buf1 = queue.Dequeue(1); // [1]
var buf2 = queue.Dequeue(2); // [2,3]
```
- **SocketUri**
```csharp
var uri1 = new SocketUri("[::]:1025");//IPv6Any
var uri2 = new SocketUri("0.0.0.0:1025");//Any
var uri3 = new SocketUri("[::1]:1025");//IPv6Loopback
var uri4 = new SocketUri("127.0.0.1:1025");//Loopback
```

*<u>NetPs以实现现有网络协议库为目标，为数据交互提供基础支撑。</u>*