# Wol (Wake On LAN)
唤醒局域网设备。默认情况下 ```WolSender``` 会向所有正常使用的网络接口发送三次wol广播包，其中广播域由接口子网掩码产生。

## 01. 先决条件
- 主板、网卡支持并开启Wake On Lan。
- 系统中开启了网卡Wake On Lan。
- 目标主机正常通电可访问，与唤醒方在同一个广播域中，并未被中间设备丢弃。

## 02. 数据包格式
- **06 byte**:	FF FF FF FF FF FF
- **96 byte**:	16* MAC Address；重复十六次MAC地址

## 03. 示例与参数
使用 ```namespace NetPs.Udp.Wol``` 下的 ```WolSender``` 类进行WOL包的发送。
```cscript
var wol = new WolSender();
wol.Send("00-00-00-00-00-08");
```

- ```int ReTimes { get; }``` 发送次数
- ```void Send(string mac);``` 发送指定次数的WOL包
- ```void SetReTimes(int times);``` 设置发送次数