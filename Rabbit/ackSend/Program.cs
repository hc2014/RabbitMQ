using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ackSend
{
    class Program
    {
        static void Main(string[] args)
        {
            //var factory = new ConnectionFactory();
            //factory.HostName = "localhost";
            //factory.UserName = "kia";
            //factory.Password = "123456";

            //using (var connection = factory.CreateConnection())
            //{
            //    using (var channel = connection.CreateModel())
            //    {
            //        channel.QueueDeclare("hello", false, false, false, null);
            //        var properties = channel.CreateBasicProperties();
            //        properties.DeliveryMode = 2;

            //        var body = Encoding.UTF8.GetBytes("Hello World!");
            //        for (int i = 0; i < 5; i++)
            //        {
            //            channel.BasicPublish("", "hello", properties, body);
            //        }
            //        Console.WriteLine(" set Hello World!");
            //    }
            //}


            //var MQHost = ConfigurationSettings.AppSettings["MQHost"].ToString();
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

                    //创建一个新的，持久的交换区 
                    channel.ExchangeDeclare("KiaExchange", ExchangeType.Fanout, true, false, null);
                    //创建一个新的，持久的队列 
                    channel.QueueDeclare("KiaQueue1", true, false, false, null);
                    //绑定队列到交换区 
                    channel.QueueBind("KiaQueue1", "KiaExchange", "");
                    using (var subscription = new Subscription(channel, "KiaQueue1", false))
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


            Console.ReadKey();
        }
    }
}
