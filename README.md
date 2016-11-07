# RabbitMQ
消息队列

#一、基础命令
安装完成RabbitMQ以后，会有一个默认的账户Guest，这个用户得删除,然后新建一个自己的用户，并且赋予管理员权限

####1.新建用户
cmd->到RabbitMQ的安装目录下的一个Sbin目录,正常情况下安装完成后在开始菜单栏可以找到这个目录
![](/RabbitImg/4.png)

```
rabbitmqctl  add_user  hc 123456
```
![](/RabbitImg/CreateUser.png)
这样就创建了一个username=hc,password=123456的用户了

####2.创建虚拟主机
虚拟主机这个概念,很不好懂，看了别人的博客,可以把这个概念理解成c#里面的“名称空间”
```
rabbitmqctl  add_vhoust  hc_mq
```
这里就创建了一个名称为hc_mq的虚拟主机

![](/RabbitImg/Createvhost.png)

####3.给用户、虚拟主机赋予权限
```
rabbitmqctl  set_permissions  -p hc_mq hc  ".*"  ".*"  ".*"
```
![](/RabbitImg/SettingPermissions2.png)
这里就为hc用户,hc_mq的虚拟主机赋予了所有权限,当然用户跟虚拟主机的权限也可以单独分开来赋权限

####4.设置角色
```
rabbitmqctl  set_user_tags yy administrator
```
![](/RabbitImg/SettingUserTags.png)
这里就给hc用户设置了超级管理员的角色


####5.删除默认用户
```
rabbitmqctl delete_user guest
```
![](/RabbitImg/DeleteGuest.png)


####6.查看用户信息

```
rabbitmqctl limt_user_permissions hc
```
![](/RabbitImg/GetUserInfo.png)



#二、普通模式发送、接受消息

####1.新建一个控制台程序作为发送端Send
```
static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "hc", Password="123456" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "hello",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
```

####2.创建一个控制台程序作为接受端Revice

```
static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "hc", Password = "123456" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };
                channel.BasicConsume(queue: "hello",
                                     noAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();

            }
        }
```
> 先启动Send就会显示发送成功
![](/RabbitImg/Send.png)

>这个是可以查看RabbitMq服务器上的消息
```
rabbitmqctl list_queues
```
![](/RabbitImg/MsgCount.png)
> 本来消息数量是0条，启动发送端以后就变成1条了

> 然后启动接收端
![](/RabbitImg/Revice.png)
 
> 再看一次服务器上 消息
![](/RabbitImg/MsgCount2.png)
> 本来消息数量是1条，启动接受端以后就又变成0条了

