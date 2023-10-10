# Wol (Wake On LAN)
唤醒局域网设备。 WolSender 会向所有正常使用的网络接口发送三次wol广播包，其中广播域由接口子网掩码产生。

## 01. 先决条件
- 主板、网卡支持并开启Wake On Lan。
- 系统中开启了网卡Wake On Lan。
- 目标主机正常通电可访问，与唤醒方在同一个广播域中，并未被中间设备丢弃。

## 02. 数据包格式
- **06 byte**:	FF FF FF FF FF FF
- **96 byte**:	16* MAC Address