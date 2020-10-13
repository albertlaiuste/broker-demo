using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrokerDemo.Server.Broker;

namespace BrokerDemo.Server
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.SetOut(new PrefixedWriter());
            
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
                    Console.WriteLine("Added a new message to database: Id: [{0}], description: {1}", e.Message.Id, e.Message.Description);
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
                Console.WriteLine("Got read from database request for ID: [{0}], with reply queue: [{1}]", e.Message.Id, e.ReplyQueue);
                var match = database.FirstOrDefault(message => message.Id == e.Message.Id);
                if (match == null)
                {
                    Console.WriteLine("No match found in database for ID: [{0}], aborting request", e.Message.Id);
                    return;
                }
                connection.PublishRead(e.ReplyQueue, match);
                Console.WriteLine("Match found, message written to queue: [{0}],\n" +
                                  "Id: [{1}], description: [{2}], updated: [{3}]", e.ReplyQueue, match.Id, match.Description, match.Updated);

            };
            
            Console.ReadKey();
            
            connection.Close();
            Console.WriteLine("Disconnected");
        }
        
        private class PrefixedWriter : TextWriter
        {
            private readonly TextWriter _originalOut;

            public PrefixedWriter()
            {
                _originalOut = Console.Out;
            }

            public override Encoding Encoding => new System.Text.ASCIIEncoding();

            public override void WriteLine(string message)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                _originalOut.Write("[Server] ");
                Console.ResetColor();
                _originalOut.WriteLine(message);
            }
            public override void Write(string message)
            {
                _originalOut.Write(message);
            }
        }
    }
}