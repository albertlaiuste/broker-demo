using System;
using  RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Google.Protobuf;

namespace BrokerDemo.Server.Broker
{
    public class BrokerConnection : IBrokerConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;

        public delegate void WriteToDatabaseEventHandler(object o, WriteToDatabaseEventArgs e);
        public event WriteToDatabaseEventHandler WriteToDatabaseEvent;
        public delegate void ReadFromDatabaseEventHandler(object o, ReadFromDatabaseEventArgs e);
        public event ReadFromDatabaseEventHandler ReadFromDatabaseEvent;
        
        public BrokerConnection()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };
        }

        public void Open()
        {
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare("demo.read", ExchangeType.Direct);
            _channel.ExchangeDeclare("demo.write", ExchangeType.Direct);
        }

        public void Close()
        {
            _channel.Close();
            _connection.Close();
        }

        public void PublishRead(string queueName, API.DemoMessage message)
        {
            var body = message.ToByteArray();
            
            var properties = _channel.CreateBasicProperties();
            properties.Type = API.DemoMessage.Descriptor.Name;
            
            _channel.BasicPublish(exchange: "",
                                  routingKey: queueName,
                                  basicProperties: properties,
                                  body: body);
        }

        public void SubscribeWrite()
        {
            _channel.QueueDeclare(queue: "demo.write.single",
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false);
            
           _channel.QueueBind(queue: "demo.write.single",
                              exchange: "demo.write",
                              routingKey: "single");
            
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (o, e) =>
            {
                if (!string.Equals(API.DemoMessage.Descriptor.Name, e.BasicProperties.Type, StringComparison.OrdinalIgnoreCase))
                    return;
                
                var message = API.DemoMessage.Parser.ParseFrom(e.Body.ToArray());
                WriteToDatabaseEvent?.Invoke(this, new WriteToDatabaseEventArgs(message));
            };

            _channel.BasicConsume(queue: "demo.write.single",
                                  autoAck: true,
                                  consumer: consumer);
        }

        public void SubscribeRead()
        {
            _channel.QueueDeclare(queue: "demo.read.single",
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false);

            _channel.QueueBind(queue: "demo.read.single",
                               exchange: "demo.read",
                               routingKey: "single");
            
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (o, e) =>
            {
                if (!string.Equals(API.DemoMessage.Descriptor.Name, e.BasicProperties.Type, StringComparison.OrdinalIgnoreCase))
                    return;
                
                var message = API.DemoMessage.Parser.ParseFrom(e.Body.ToArray());
                ReadFromDatabaseEvent?.Invoke(this, new ReadFromDatabaseEventArgs(e.BasicProperties.ReplyTo, message));
            };

            _channel.BasicConsume(queue: "demo.read.single",
                                  autoAck: true,
                                  consumer: consumer);
        }
    }
    
    public class WriteToDatabaseEventArgs
    {
        public WriteToDatabaseEventArgs(API.DemoMessage message)
        {
            Message = message;
        }
        public API.DemoMessage Message { get; }
    }
    
    public class ReadFromDatabaseEventArgs
    {
        public ReadFromDatabaseEventArgs(string replyQueue, API.DemoMessage message)
        {
            ReplyQueue = replyQueue;
            Message = message;
        }
        public string ReplyQueue { get; }
        public API.DemoMessage Message { get; }
    }
}