# lifetime

## 01.TcpClient

![image](TcpClient.png)

## 02.TcpServer

![image](TcpServer.png)

# base

## 01. TcpTx

- **发送队列**：添加数据，无需等待发送完成。
- **重传**：接收方不能及时接收，再次发送未完成数据。

![image](TcpTx.png)

## 02. TcpRx

- **数据接收**：每次接收数据发送通知事件。

![image](TcpRx.png)

# limit speed

## 01. TcpRxRepeater

- **转发**：将Rx接收的数据转发到任意实现 ```IDataTransport``` 接口的实例中。
- **流控制**：限制发送接收带宽，由发送完成事件驱动 再次接收。

![image](TcpRxRepeater.png)

## 02. TcpLimitRx

- **流控制**：限制接收数据带宽。

![image](TcpLimitRx.png)

## 03. TcpLimitTx

- **流控制**：限制发送数据带宽。

![image](TcpLimitTx.png)