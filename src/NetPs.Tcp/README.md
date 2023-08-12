## 简介

NetPs.TCP基于NetPs.Socket，对TCP数据数据传输提供便捷。

其中将服务端、客户端 分开进行实现TcpServer类与TcpClient类。并且提供了一种转发原有Tcp数据的方法。

## How To Use?

- **TcpServer**
  
```csharp
var server = new TcpServer((server, client) =>
{
  // Accept new client
  // 转发到192.168.1.1，并且会将接收后的数据转发回请求端
  client.StartMirror("192.168.1.1:443");
});
server.Run("0.0.0.0:7070", () =>  Environment.Exit(0));
```

- **TcpClient**
  
```csharp
var client = new TcpClient();
client.ReceivedObservable.Subscribe(data => {
  // received byte[] data
});
client.ConnectedObservable.Subscribe(_ => client.Rx.StartReveice());
client.Connect("127.0.0.1:12345");
client.Tx.Transport(new byte[] {1,2,3});
```


*<u>NetPs以实现现有网络协议库为目标，为数据交互提供基础支撑。</u>*