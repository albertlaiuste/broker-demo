using System;
using System.Collections.Generic;
using System.Linq;
using BrokerDemo.Server.Broker;

namespace BrokerDemo.Server
{
    static class Program
    {
        static void Main(string[] args)
        {
            var database = new List<API.DemoMessage>();
            
            var connection = new BrokerConnection();
            connection.Open();
            Console.WriteLine("Connected");
            
            connection.SubscribeRead();
            connection.SubscribeWrite();
            Console.WriteLine("Subscribed to \'demo.read.single\' and \'demo.write.single\'");
            
            connection.WriteToDatabaseEvent += (o, e) =>
            {
                var match = database.FirstOrDefault(message => message.Id == e.Message.Id);
                if (match == null)
                {
                    database.Add(e.Message);
                    Console.WriteLine("Added new message to database: Id: [{0}], description: {1}", e.Message.Id, e.Message.Description);
                }
                else
                {
                    match.Description = e.Message.Description;
                    match.Updated = e.Message.Updated;
                    Console.WriteLine("Updated message: Id: [{0}], description: {1}, updated: {2}", e.Message.Id, e.Message.Description, e.Message.Updated.ToDateTime());
                }
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
            
            Console.ReadKey();
            
            connection.Close();
            Console.WriteLine("Disconnected");
        }
    }
}