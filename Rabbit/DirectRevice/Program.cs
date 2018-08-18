using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectRevice
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "kia",
                Password = "123456",
                Protocol = Protocols.DefaultProtocol,
                RequestedFrameMax = UInt32.MaxValue,
                RequestedHeartbeat = UInt16.MaxValue
            };

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
               var queueName = channel.QueueDeclare().QueueName;

                Console.WriteLine("请输入Key:");
                var Key = Console.ReadLine();

                channel.QueueBind(queue: queueName,
                                  exchange: "TestDirectExChange",
                                  routingKey: Key);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [x] Received '{0}':'{1}'",
                                      routingKey, message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }


            //Task.Factory.StartNew(() =>
            //{
            //    using (var connection = connectionFactory.CreateConnection())
            //    using (var channel = connection.CreateModel())
            //    {
            //        // 这指示通道不预取超过1个消息 
            //        channel.BasicQos(0, 1, false);

            //        //创建一个新的，持久的交换区 
            //        channel.ExchangeDeclare("TestDirectExChange", ExchangeType.Direct, true, false, null);
            //        //创建一个新的，持久的队列 
            //        channel.QueueDeclare(queueName, true, false, false, null);
            //        //绑定队列到交换区 
            //        channel.QueueBind(queueName, "TestDirectExChange", roleName);
            //        using (var subscription = new Subscription(channel, queueName, false))
            //        {
            //            Console.WriteLine("等待消息...");
            //            var encoding = new UTF8Encoding();
            //            while (channel.IsOpen)
            //            {
            //                BasicDeliverEventArgs eventArgs;
            //                var success = subscription.Next(2000, out eventArgs);
            //                if (success == false) continue;
            //                try
            //                {
            //                    var msgBytes = eventArgs.Body;
            //                    var message = encoding.GetString(msgBytes);
            //                    Console.WriteLine(message);
            //                    channel.BasicAck(eventArgs.DeliveryTag, false);
            //                }
            //                catch (Exception ex)
            //                {
            //                    channel.BasicNack(eventArgs.DeliveryTag, false, true);
            //                }
            //            }
            //        }
            //    }
            //});

            Console.WriteLine("处理完成");
            Console.ReadKey();
        }
    }
}
