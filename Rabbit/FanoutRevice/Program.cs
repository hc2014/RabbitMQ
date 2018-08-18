using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FanoutRevice
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("请输入消费者队列的名称:");
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
            Task.Factory.StartNew(() => {
                using (var connection = connectionFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    // 这指示通道不预取超过1个消息 
                    channel.BasicQos(0, 1, false);
                    
                    //创建一个新的，持久的队列 
                    channel.QueueDeclare(queueName, true, false, false, null);
                    //绑定队列到交换区 
                    channel.QueueBind(queueName, "TestExChange", "");
                    using (var subscription = new Subscription(channel, queueName, false))
                    {
                        Console.WriteLine("等待消息...");
                        var encoding = new UTF8Encoding();
                        while (channel.IsOpen)
                        {
                            BasicDeliverEventArgs eventArgs;
                            var success = subscription.Next(2000, out eventArgs);
                            if (success == false) continue;
                            try
                            {
                                var msgBytes = eventArgs.Body;
                                var message = encoding.GetString(msgBytes);
                                Console.WriteLine(message);
                                channel.BasicAck(eventArgs.DeliveryTag, false);
                            }
                            catch (Exception ex)
                            {
                                channel.BasicNack(eventArgs.DeliveryTag, false, true);
                            }

                        }
                    }
                }
            });

            Console.WriteLine("处理完成");
            Console.ReadKey();
        }
    }
}
