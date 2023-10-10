### 01. TcpReapterTest
测试 tcp端口转发，将指定网络地址转发到当前主机的端口上。

观测目标：可以正常访问转发后的服务。
```
 var test = new TcpReapterTest("172.17.0.1:80", "0.0.0.0:3021");
```

### 02. UdpTest
测试两个UdpHost 间是否可以正常通信。

观测目标：能正常接收与发送数据。
```
 var test = new UdpTest();
```

### 03. WolTest
测试发送 广播域中其它机器是否可以收到wol数据包。

观测目标：广播域中其它机器抓取到wol包。
```
var test = new WolTest();
```