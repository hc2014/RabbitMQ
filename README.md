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
**先启动Send就会显示发送成功**<br />
![](/RabbitImg/Send.png)<br />

**这个是可以查看RabbitMq服务器上的消息**<br />
```
rabbitmqctl list_queues
```
![](/RabbitImg/MsgCount.png)<br />
**本来消息数量是0条，启动发送端以后就变成1条了**<br />

**然后启动接收端**<br />
![](/RabbitImg/Revice.png)<br />
 
**再看一次服务器上 消息**<br />
![](/RabbitImg/MsgCount2.png)<br />
**本来消息数量是1条，启动接受端以后就又变成0条了**<br />



#三、工作队列(分发模式)

####1.新建接收端,完成后打开两个处于等待发送端发送消息 状态
```
 static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.HostName = "localhost";
            factory.UserName = "hc";
            factory.Password = "123456";

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("hello", false, false, false, null);

                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("hello", true, consumer);

                    while (true)
                    {
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                        int dots = message.Split('.').Length - 1;
                        Thread.Sleep(dots * 1000);

                        Console.WriteLine("Received {0}", message);
                        Console.WriteLine("Done");
                    }
                }
            }
        }
```

####2.新建发送端
```
 static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.HostName = "localhost";
            factory.UserName = "hc";
            factory.Password = "123456";

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("hello", false, false, false, null);
                    string message = GetMessage(args);
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2;

                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish("", "hello", properties, body);
                    Console.WriteLine(" set {0}", message);
                }
            }

            Console.ReadKey();

        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }
```

**直接用cmd 模式来发送5条数据**<br />
![](/RabbitImg/1.png)<br />
**这个时候两个接收端都分别收到了数据**<br />
![](/RabbitImg/2.png)<br />
**特别注意的是,服务端一共发了5条数据,而两个接收端分别收到3条跟2条数据,这个过程是由Rabbi自动去完成的，他自己把消息平均分发给接收端**<br />


#四、消息应答
 > 消息响应默认是开启的。在之前的例子中使用了no_ack=True标识把它关闭。现在把这个标志关掉.
 > 先设置channel.BasicConsume("hello", **false**, consumer);.
 > 然后最后加一句**channel.BasicAck(ea.DeliveryTag, false);**.
 > 发送端代码不变,接受端代码如下:
 > 执行以后，可以看出消息发送以后,在服务器队列里面hello有一条消息,然后执行接受端,等待接收端执行完成以后,消息就没了！.
![](/RabbitImg/ack1.png)

**这种情况是正常的,但是假如说接收端正在执行 时候、或者说是没有返回给服务端信息的话又是怎样的结果呢？**、
> 先拿到**channel.BasicAck(ea.DeliveryTag, false);**
![](/RabbitImg/ack2.png)
