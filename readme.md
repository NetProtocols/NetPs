# NetPs(Net protocols)

*NetPs以实现现有网络协议库为目标，为数据交互提供基础支撑。*



### How to Use?

```powershell
NuGet\Install-Package NetPs.Udp
NuGet\Install-Package NetPs.Tcp
```



### Next Plan

- 流控制：控制网络带宽总的、单个的数据传输速度。

- 有效期控制：无连接后自动关闭。

- 加密：可以选择对数据流进行加密，防止中间更改。

- 压缩：可以选择对数据流进行简单压缩，可用于在传输音视频、图片等数据流。

- 请求限制：
  
  - 可以限制单用户、总最大连接数。
  
  - 可以对频繁访问后无数据传输断开，数据内容报错的地址进行限制接入；

- 日志：可以选择输出日志内容到数据库、文件中。

- TLS：NetPs.TCP中增加TLSv1.0、TLSv1.1、TLSv1.2、TLSv1.3的实现。




