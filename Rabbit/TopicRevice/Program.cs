using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TopicRevice
{
    class Program
    {
        const string TopExchangeName = "topic.justin.exchange";
        //const string TopQueueName = "topic.justin.queue";

        static void Main(string[] args)
        {

            Console.WriteLine("请输入路由的名称:");
            var queueName = Console.ReadLine();
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "kia",
                Password = "123456",
                Protocol = Protocols.AMQP_0_9_1,
                RequestedFrameMax = UInt32.MaxValue,
                RequestedHeartbeat = UInt16.MaxValue
            };

            using (IConnection conn = connectionFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    //channel.ExchangeDeclare(TopExchangeName, "topic", durable: false, autoDelete: false, arguments: null);

                    var TopQueueName = channel.QueueDeclare().QueueName;
                    Console.WriteLine($"TopQueueName:{TopQueueName}");
                    var TopQueueName1 = channel.QueueDeclare().QueueName;


                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    channel.QueueBind(TopQueueName, TopExchangeName, routingKey: queueName);

                    //这里可以监听多个路由
                    if (queueName == "order.delete.user")
                    {
                        channel.QueueBind(TopQueueName, TopExchangeName, routingKey: "order.update.admin");

                        
                        Console.WriteLine($"TopQueueName1:{TopQueueName1}");
                        channel.QueueBind(TopQueueName1, "TestExChange", "");
                        
                    }



                   



                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var msgBody = Encoding.UTF8.GetString(ea.Body);
                        Console.WriteLine(string.Format("***接收时间:{0}，消息内容：{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msgBody));
                        int dots = msgBody.Split('.').Length - 1;
                        System.Threading.Thread.Sleep(dots * 1000);
                        Console.WriteLine(" [x] Done");
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        Thread.Sleep(500);
                    };
                    channel.BasicConsume(TopQueueName, autoAck: false, consumer: consumer);
                    if (queueName == "order.delete.user")
                    {
                        channel.BasicConsume(TopQueueName1, autoAck: false, consumer: consumer);

                    }
                    Console.WriteLine("按任意值，退出程序");
                    Console.ReadKey();
                }
            }


            Console.WriteLine("处理完成");
            Console.ReadKey();
        }
    }
}
