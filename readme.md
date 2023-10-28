# NetPs(Net protocols)

[![license](https://img.shields.io/github/license/NetProtocols/NetPs)](https://github.com/NetProtocols/NetPs/blob/main/LICENSE)
[![NetPs v1.0](https://github.com/NetProtocols/NetPs/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/NetProtocols/NetPs/actions/workflows/dotnet-desktop.yml)

*NetPs以实现现有网络协议库为目标，为数据交互提供基础支撑。*

### How Use?

```powershell
NuGet\Install-Package NetPs.Udp
NuGet\Install-Package NetPs.Tcp
```

### How Can?
- **转发**：将tcp\udp 转发到 ```IDataTransport``` 中。
- **限流**：限制接收、发送、转发的流速。

### Next Plan

- 流控制：控制网络带宽总的、~~单个的~~数据传输速度。

- 有效期控制：无连接后自动关闭。

- 加密：可以选择对数据流进行加密，防止中间更改。

- 压缩：可以选择对数据流进行简单压缩，可用于在传输音视频、图片等数据流。

- 请求限制：
  
  - 实现IP DenyList、AllowList。
  
  - 可以限制单用户、总最大连接数。
  
  - 可以对频繁访问后无数据传输断开，数据内容报错的地址进行限制接入；

- 日志：可以选择输出日志内容到数据库、文件中。

- TLS：增加TLSv1.0、TLSv1.1、TLSv1.2、TLSv1.3的实现。

- 速度检测：测试当前连接的码率、 最大带宽、时延等数据。


### 文档
1. [socket 说明文档](src/NetPs.Socket/readme.md)  
1.1 [security 信息安全](src/NetPs.Socket/Extras/Security/readme.md)  
2. [tcp 说明文档](src/NetPs.Tcp/readme.md)  
3. [udp 说明文档](src/NetPs.Udp/readme.md)  
3.1 [WOL 网络唤醒](src/NetPs.Udp/Wol/readme.md)  
3.2 [DNS 客户端](src/NetPs.Udp/DNS/readme.md)  