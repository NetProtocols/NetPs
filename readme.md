# NetPs(Net protocols)
[![NetPs v1.0](https://github.com/NetProtocols/NetPs/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/NetProtocols/NetPs/actions/workflows/dotnet-desktop.yml)

*NetPs以实现现有网络协议库为目标，为数据交互提供基础支撑。*

### How Use?

```powershell
NuGet\Install-Package NetPs.Udp
NuGet\Install-Package NetPs.Tcp
```

### How Can?
- **转发**：将tcp\udp 转发到任意实现 ```IDataTransport``` 的实例中。
- **限流**：限制接收、发送、转发的网络速度。

### Flows
**Tcp**: [基础功能流程图](src\NetPs.Tcp\doc\readme.md)、[未来功能](src\NetPs.Tcp\doc\feture.md)

### Next Plan

- 流控制：控制网络带宽总的、~~单个的~~数据传输速度。

- 有效期控制：无连接后自动关闭。

- 加密：可以选择对数据流进行加密，防止中间更改。

- 压缩：可以选择对数据流进行简单压缩，可用于在传输音视频、图片等数据流。

- 请求限制：
  
  - 可以限制单用户、总最大连接数。
  
  - 可以对频繁访问后无数据传输断开，数据内容报错的地址进行限制接入；

- 日志：可以选择输出日志内容到数据库、文件中。

- TLS：NetPs.TCP中增加TLSv1.0、TLSv1.1、TLSv1.2、TLSv1.3的实现。
