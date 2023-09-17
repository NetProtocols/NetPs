# Wol (Wake On LAN)
唤醒局域网设备。

## 01. 先决条件
- 主板、网卡支持Wake On Lan。
- 系统中开启了网卡Wake On Lan。
- 目标主机正常通电可访问，跨网段时广播未被中间设备丢弃。

## 02. 数据包格式
- **06 byte**:	FF FF FF FF FF FF

- **16 byte**:	MAC Address