using System;
using System.Threading.Tasks;
using Client.Broker;

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
            connection.Produce(10, "First message", DateTime.Now);
            connection.Produce(20, "Second message", DateTime.Now);
            connection.Produce(30, "Third message", DateTime.Now);
            Console.WriteLine("Produced messages");
            // Ask for some message from queue
            var received1 = await Requests.RequestMessage(connection, 20);
            Console.WriteLine("Received message: [{0}] [{1}], Updated: {2}", received1.Id, received1.Description, received1.Updated);
            var received2 = await Requests.RequestMessage(connection, 30);
            Console.WriteLine("Received message: [{0}] [{1}], Updated: {2}", received2.Id, received2.Description, received2.Updated);
            connection.Close();
            Console.WriteLine("Disconnected");
        }
    }
}