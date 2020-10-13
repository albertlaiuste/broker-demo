using System;
using  RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Client.Broker
{
    public class BrokerConnection : IBrokerConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;

        private string _queueName;

        public delegate void MessageReceivedEventHandler(object o, MessageReceivedEventArgs e);
        public event MessageReceivedEventHandler MessageReceivedEvent;

        public BrokerConnection()
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName    = "localhost",
                Port        = 5672,
                UserName    = "guest",
                Password    =  "guest",
                VirtualHost = "/"
            };
        }
        public void Open()
        {
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        public void Close()
        {
            _channel.Close();
            _connection.Close();
        }
        public void Produce(BrokerDemo.API.DemoMessage message)
        {
            var body = message.ToByteArray();
            
            var properties = _channel.CreateBasicProperties();
            properties.Type = BrokerDemo.API.DemoMessage.Descriptor.Name;

            _channel.BasicPublish(exchange: "demo.write",
                                  routingKey: "single",
                                  basicProperties: properties,
                                  body: body);
        }
        public void Request(int id)
        {
            var body = new BrokerDemo.API.DemoMessage
            {
                Id = id
            }
            .ToByteArray();

            var properties = _channel.CreateBasicProperties();
            properties.ReplyTo = _queueName;
            properties.Type = BrokerDemo.API.DemoMessage.Descriptor.Name;
            
            _channel.BasicPublish(exchange: "demo.read",
                                  routingKey: "single",
                                  basicProperties: properties,
                                  body: body);
        }
        public void Subscribe()
        {
            _queueName = _channel.QueueDeclare(queue: "",
                                               durable: false,
                                               exclusive: true,
                                               autoDelete: true,
                                               arguments: null)
                                              .QueueName;
            
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += OnMessageReceived;

            _channel.BasicConsume(queue: _queueName,
                                  autoAck: true,
                                  consumer: consumer);
        }
        private void OnMessageReceived(object o, BasicDeliverEventArgs e)
        {
            Console.WriteLine("Message received");
            if (!string.Equals(BrokerDemo.API.DemoMessage.Descriptor.Name, e.BasicProperties.Type,
                StringComparison.OrdinalIgnoreCase)) return;
            
            var message = BrokerDemo.API.DemoMessage.Parser.ParseFrom(e.Body.ToArray());
            MessageReceivedEvent?.Invoke(this, new MessageReceivedEventArgs(message));
        }
    }
    public class MessageReceivedEventArgs
    {
        public MessageReceivedEventArgs(BrokerDemo.API.DemoMessage message)
        {
            Message = message;
        }
        public BrokerDemo.API.DemoMessage Message { get; }
    }
}