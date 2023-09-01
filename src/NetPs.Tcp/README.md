## 简介

NetPs.TCP基于NetPs.Socket，对TCP数据数据传输提供便捷。

其中将服务端、客户端 分开进行实现```TcpServer```类与```TcpClient```类。

增加了```TcpClient``` 的分支 ```TcpRepeaterClient``` 优化转发类，在此基础上提供了```StartMirror(url)```方法。


该库支持提前预热来加快首次运行速度，可以通过调用方法进行使用 `NetPs.Socket.Eggs.Food.Heating();`
## How To Use?

### 01. TcpServer
  
当 `"0.0.0.0:0"` 中端口为 `"0"` 值时，会生成可使用的端口，你可以通过`server.Address`、`server.IPEndPoint` 查看。
  
你可以通过的两种快捷方式创建该实例。
  
- *1. parament*
```csharp
var server = new TcpServer((server, client) =>
{
  // Accept new client
  // 转发到192.168.1.1，并且会将接收后的数据转发回请求端
  client.StartMirror("192.168.1.1:443");
});
server.Run("0.0.0.0:7070", () =>  Environment.Exit(0));
```

- *2. interface* 
```csharp
class Server : ITcpServerConfig, IDisposable
{
    private TcpServer serv;
    public Server()
    {
        this.serv = new TcpServer(this);
    }

    public string BandAddress => "0.0.0.0:0";

    public bool TcpAccept(TcpServer server, TcpClient client) { 
        //this.serv.StartReceive(this); //需要手动开启接收
        tcp.StartMirror("192.168.1.1:443");
        return true;
    }
    public void TcpConfigure(TcpCore core) { }

    public void TcpReceive(byte[] data, TcpClient tcp) {}
    
    public void Dispose()
    {
        this.serv.Dispose();
    }
}
```


### 02.TcpClient
```csharp
var client = new TcpClient();
client.ReceivedObservable.Subscribe(data => {
  // received byte[] data
});
client.ConnectedObservable.Subscribe(_ => client.Rx.StartReceive());
client.Connect("127.0.0.1:12345");
client.Transport(new byte[] {1,2,3});
```

### 03.TcpRepeaterClient
- **Limit**：可限制流速，如1秒最大转发10M ```client.SetLimit(10<<20);``` 。

```
// A, B之间转发数据
var A = new TcpClient();
var B = new TcpClient();
var repeater_client1 = new TcpRepeaterClient(B, B.Tx);
var repeater_client2 = new TcpRepeaterClient(A, A.Tx);
repeater_client1.Limit(10 << 20);
repeater_client2.Limit(10 << 20);

repeater_client1.Disposables.Add(repeater_client1.ConnectedObservable.Subscribe(_ =>
{
    repeater_client1.Rx.StartReceive();
    repeater_client2.Rx.StartReceive();
}));
```


*<u>NetPs以实现现有网络协议库为目标，为数据交互提供基础支撑。</u>*