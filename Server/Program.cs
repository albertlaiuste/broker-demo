using System;

using System.Collections.Generic;
using System.Linq;
using Server.Broker;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var database = new List<BrokerDemo.API.DemoMessage>();
            
            var connection = new BrokerConnection();
            connection.Open();
            Console.WriteLine("Connected");
            connection.SubscribeRead();
            connection.SubscribeWrite();
            Console.WriteLine("Subscribed to \'demo.read.single\' and \'demo.write.single\'");
            connection.WriteToDatabaseEvent += (o, e) =>
            {
                database.Add(e.Message);
                Console.WriteLine("Message received to store: Id: [{0}], description: {1}", e.Message.Id, e.Message.Description);
            };
            connection.ReadFromDatabaseEvent += (o, e) =>
            {
                var match = database.FirstOrDefault(message => message.Id == e.Message.Id);
                if (match == null)
                    return;
                
                connection.PublishRead(e.ReplyQueue, match);
                Console.WriteLine("Message written to queue: [{0}], " +
                                  "Id: [{1}], description: {2}", e.ReplyQueue, match.Id, match.Description);

            };
            while (Console.ReadKey().Key != ConsoleKey.Enter)
            {
                connection.Close();
                Console.WriteLine("Disconnected");
            }
        }
    }
}