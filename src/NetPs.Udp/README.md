## 简介

NetPs.Udp基于NetPs.Socket，对UDP数据数据传输提供便捷。

其中实现了UdpHost、DnsHost、WolSender。

该库支持提前预热来加快首次运行速度，可以通过调用方法进行使用 `NetPs.Socket.Eggs.Food.Heating();`
## How To Use?

- **UdpHost**
  
  当 `"0.0.0.0:0"` 中端口为 `"0"` 值时，会生成可使用的端口，你可以通过`host.Address`、`host.IPEndPoint` 查看。
```csharp
var host = new UdpHost("0.0.0.0:12345");
host.StartReveice(data =>
{
    using (var tx = host.GetTx(data.IP))
    {
        tx.Transport(new byte[] { 1, 2 });
    }
});
using (var tx = host.GetTx("192.168.1.1:12345"))
{
    tx.Transport(new byte[] { 1, 2 });
}
```

- **UdpMirrorHub**  
示例为转发dns请求指定服务器。
```
var host = new UdpHost("0.0.0.0:0");
host.Rx.NoBufferReceived += (buffer, length, address) =>
{
    var c = this.host.Clone(address);
    c.StartHub(new UdpMirrorHub(c, "114.114.114.114:53", 10 << 20));
    return false;
};
host.StartReceive();
```

- **DnsHost**
  
  支持中文及特殊域名，你也可以手动调用方法进行编解码 `Punycode.Encode(url);`、`Puncode.Decode(url);` 。

  该方法无法防止DNS被劫持的情况，如果你使用openwrt 可以取消 劫持53端口的防火墙配置，来使你设置的服务端地址生效。

```csharp
var host = new DnsHost();
var packet = await host.SendReqA($"{DnsHost.DNS_ALI}:53", "nuget.org");
```

- **WolSender**
    
    向本地所有接口发送三次网络唤醒包WakeOnLan Packet.
```csharp
var wol = new WolSender();
wol.Send("00:00:00:00:00:0e");
```

- **SpeedTest**
    

```
//    网速测试，将在下个版本释放。
```

*<u>NetPs以实现现有网络协议库为目标，为数据交互提供基础支撑。</u>*