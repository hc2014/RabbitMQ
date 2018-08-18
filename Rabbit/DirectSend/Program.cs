using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectSend
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory client;
            client = new ConnectionFactory();
            client.HostName = "localhost";
            client.UserName = "kia";
            client.Password = "123456";

            client.RequestedFrameMax = UInt32.MaxValue;//超时时间
            client.RequestedHeartbeat = UInt16.MaxValue; //心跳超时时间 
            client.AutomaticRecoveryEnabled = true;//自动重连
            client.Protocol = Protocols.DefaultProtocol;

            using (var connection = client.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    //创建一个新的，持久的交换区 
                   channel.ExchangeDeclare("TestDirectExChange", ExchangeType.Direct, true, false, null);

                    ////创建一个新的，消息持久的队列
                    //channel.QueueDeclare("DirectQueue",true,false,false,null);

                    //channel.QueueBind("DirectQueue", "TestDirectExChange", "routingKeyOne");
                    //channel.QueueBind("DirectQueue", "TestDirectExChange", "routingKeyTwo");


                    // 设置消息属性 
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; //消息是持久的，存在并不会受服务器重启影响  
                    var encoding = new UTF8Encoding();

                    channel.BasicPublish("TestDirectExChange", "routingKeyOne", properties, encoding.GetBytes("测试消息1"));
                    channel.BasicPublish("TestDirectExChange", "routingKeyOne", properties, encoding.GetBytes("测试消息2"));
                    channel.BasicPublish("TestDirectExChange", "routingKeyOne", properties, encoding.GetBytes("测试消息3"));
                    channel.BasicPublish("TestDirectExChange", "routingKeyTwo", properties, encoding.GetBytes("测试消息4"));
                    channel.BasicPublish("TestDirectExChange", "routingKeyTwo", properties, encoding.GetBytes("测试消息5"));
                    channel.BasicPublish("TestDirectExChange", "routingKeyTwo", properties, encoding.GetBytes("测试消息6"));
                    channel.BasicPublish("TestDirectExChange", "routingKeyTwo", properties, encoding.GetBytes("测试消息7"));

                    //for (int i = 0; i < 6; i++)
                    //{
                    //    var msgBytes = encoding.GetBytes("测试消息" + i);
                    //    if (i % 2 == 0)
                    //    {
                    //        channel.BasicPublish("TestDirectExChange", "routingKeyOne", properties, msgBytes);
                    //    }
                    //    else
                    //    {
                    //        channel.BasicPublish("TestDirectExChange", "routingKeyTwo", properties, msgBytes);
                    //    }
                    //}

                    channel.Close();
                }
            }


            Console.ReadKey();
        }
    }
}
