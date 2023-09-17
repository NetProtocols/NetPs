# Udp Hole
hole 可以减小服务端资源消耗，减小两端延迟。

## 01. 网络环境
**env01. 100%成功**  clientA、clientB、s 在同一网段下。

**env02. 100%成功**  clientA、s 在同一网段下; clientB 在另一网段下。

**env03.**  clientA、clientB 在同一网段下; s 在另一网段下。

**env04.**  clientA、clientB、s 网段各不相同。

**env05.**  clientA、clientB、s 皆是外网ipv6。

## 02. 尝试
- **fail** clientA 注册 s; clientB 向s 发送 holeA; s向clientB 与 clientA 返回 hole 对方IP端口; clientA\B互联。

- **fail** ...clientB 向s 发送 holeA; s向 clientA 发送辅助验证推算真实port; ...

- **wait** ...clientB 向s 发送 holeA; s向 clientA、clientB 发送各自IP端口; clientA、clientB连接自身端口;...
	