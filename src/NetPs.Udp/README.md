## 简介

NetPs.Udp基于NetPs.Socket，对UDP数据数据传输提供便捷。

其中实现了UdpHost与DnsHost。

## How To Use?

- **UdpHost**
  
```csharp
var host = new UdpHost("0.0.0.0:12345");
host.ReceicedObservable.Subscribe(data =>
{
    using (var tx = host.GetTx(data.IP))
    {
        tx.Transport(new byte[] { 1, 2 });
    }
});
host.Rx.StartReveice();
using (var tx = host.GetTx("192.168.1.1:12345"))
{
    tx.Transport(new byte[] { 1, 2 });
}
```

- **DnsHost**
  
```csharp
var host = new DnsHost();
var packet = await host.SendReqA("223.6.6.6:53", "nuget.org");
```


*<u>NetPs以实现现有网络协议库为目标，为数据交互提供基础支撑。</u>*