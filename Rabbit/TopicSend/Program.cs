using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopicSend
{
    class Program
    {
        const string TopicName1 = "order.add.admin";//管理员添加订单
        const string TopicName2 = "order.delete.user";//用户删除订单
        const string TopicName3 = "order.update.admin";//管理修改订单

        const string TopExchangeName = "topic.justin.exchange";
        const string TopQueueName = "topic.justin.queue";

        static void Main(string[] args)
        {
            ConnectionFactory client;
            #region 创建连接
            client = new ConnectionFactory();
            client.HostName = "localhost";
            client.UserName = "kia";
            client.Password = "123456";

            client.RequestedFrameMax = UInt32.MaxValue;//超时时间
            client.RequestedHeartbeat = UInt16.MaxValue; //心跳超时时间 
            client.AutomaticRecoveryEnabled = true;//自动重连
            client.Protocol = Protocols.DefaultProtocol; 
            #endregion

            using (IConnection conn = client.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(TopExchangeName, "topic", durable: false, autoDelete: false, arguments: null);
                    //channel.QueueDeclare(TopQueueName, durable: false, autoDelete: false, exclusive: false, arguments: null);
                    //channel.QueueBind(TopQueueName, TopExchangeName, routingKey: TopicName1);
                    //channel.QueueBind(TopQueueName, TopExchangeName, routingKey: TopicName2);
                    //channel.QueueBind(TopQueueName, TopExchangeName, routingKey: TopicName3);

                    string vadata = Console.ReadLine();
                    while (vadata != "exit")
                    {
                        var msgBody = Encoding.UTF8.GetBytes(vadata);
                        if (vadata.ToString().StartsWith("1"))
                        {
                            channel.BasicPublish(exchange: TopExchangeName, routingKey: TopicName1, basicProperties: null, body: msgBody);
                        }
                        else if (vadata.ToString().StartsWith("2"))
                        {
                            channel.BasicPublish(exchange: TopExchangeName, routingKey: TopicName2, basicProperties: null, body: msgBody);
                        }
                        else if (vadata.ToString().StartsWith("3"))
                        {
                            channel.BasicPublish(exchange: TopExchangeName, routingKey: TopicName3, basicProperties: null, body: msgBody);
                        }
                        Console.WriteLine(string.Format("***发送时间:{0}，发送完成，输入exit退出消息发送", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                        vadata = Console.ReadLine();
                    }
                }
            }
            Console.ReadKey();
        }
    }
}
