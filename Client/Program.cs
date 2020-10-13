using System;
using System.Threading.Tasks;
using Client.Broker;
using Google.Protobuf.WellKnownTypes;

namespace Client
{
    static class Program
    {
        public static async Task Main(string[] args)
        {
            var connection = new BrokerConnection();
            connection.Open();
            Console.WriteLine("Connected");
            // Subscribe to any requests received
            connection.Subscribe();
            Console.WriteLine("Subscribed to channel");
            // Produce couple of messages to queue
            var msg1 = new BrokerDemo.API.DemoMessage
            {
                Id = 10,
                Description = "First message",
                Updated = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            };
            var msg2 = new BrokerDemo.API.DemoMessage
            {
                Id = 20,
                Description = "Second message",
                Updated = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            };
            connection.Produce(msg1);
            connection.Produce(msg2);
            Console.WriteLine("Produced messages");
            // Ask for some message from queue
            var received1 = await Requests.RequestMessage(connection, 10);
            Console.WriteLine("Received message: [{0}] [{1}], Updated: {2}", received1.Id, received1.Description, received1.Updated);
            connection.Close();
            Console.WriteLine("Disconnected");
        }
    }
}