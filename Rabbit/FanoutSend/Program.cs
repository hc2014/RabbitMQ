using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FanoutSend
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
                    channel.ExchangeDeclare("TestExChange", ExchangeType.Fanout, true, false, null);

                    // 设置消息属性 
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; //消息是持久的，存在并不会受服务器重启影响  

                    var encoding = new UTF8Encoding();
                    for (int i = 0; i < 6; i++)
                    {
                        var msgBytes = encoding.GetBytes("测试消息"+i);
                        channel.BasicPublish("TestExChange", "", properties, msgBytes);
                    }
                    
                    channel.Close();
                }
            }


            Console.ReadKey();
        }
    }
}
