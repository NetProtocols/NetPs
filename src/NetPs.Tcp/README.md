## 简介

NetPs.TCP基于NetPs.Socket，对TCP数据数据传输提供便捷。

其中将服务端、客户端 分开进行实现TcpServer类与TcpClient类。并且提供了一种转发原有Tcp数据的方法。


该库支持提前预热来加快首次运行速度，可以通过调用方法进行使用 `NetPs.Socket.Eggs.Food.Heating();`
## How To Use?

- **TcpServer**
  
  当 `"0.0.0.0:0"` 中端口为 `"0"` 值时，会生成可使用的端口，你可以通过`server.Address`、`server.IPEndPoint` 查看。
  
  你可以通过的两种快捷方式创建该实例。
  
*1. parament*
```csharp
var server = new TcpServer((server, client) =>
{
  // Accept new client
  // 转发到192.168.1.1，并且会将接收后的数据转发回请求端
  client.StartMirror("192.168.1.1:443");
});
server.Run("0.0.0.0:7070", () =>  Environment.Exit(0));
```

*2. interface* 
```csharp
class Server : ITcpServerConfig, IDisposable
{
    private TcpServer serv;
    public Server()
    {
        this.serv = new TcpServer(this);
    }

    public string BandAddress => "0.0.0.0:0";

    public bool TcpAccept(TcpServer server, TcpClient client) => true;

    public void TcpConfigure(TcpCore core) { }

    public void TcpReceive(byte[] data, TcpClient tcp)
    {
        tcp.StartMirror("192.168.1.1:443");
    }
    
    public void Dispose()
    {
        this.serv.Dispose();
    }
}
```


- **TcpClient**

```csharp
var client = new TcpClient();
client.ReceivedObservable.Subscribe(data => {
  // received byte[] data
});
client.ConnectedObservable.Subscribe(_ => client.Rx.StartReceive());
client.Connect("127.0.0.1:12345");
client.Transport(new byte[] {1,2,3});
```


*<u>NetPs以实现现有网络协议库为目标，为数据交互提供基础支撑。</u>*