using System;
using System.Threading;
using System.Threading.Tasks;
using BrokerDemo.Client.Broker;
using Google.Protobuf.WellKnownTypes;

namespace BrokerDemo.Client
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
            Console.WriteLine("Subscribed to to anonymous queue");
            // Produce couple of messages to queue
            var msg1 = new API.DemoMessage
            {
                Id = 10,
                Description = "First message",
                Updated = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            };
            var msg2 = new API.DemoMessage
            {
                Id = 20,
                Description = "Second message",
                Updated = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime())
            };
            connection.Produce(msg1);
            connection.Produce(msg2);
            Console.WriteLine("Produced 2 messages to queue: \'demo.write.single\'");
            // Ask for some message from queue
            try
            {
                Console.WriteLine("Sent request for queue: \'demo.read.single\' with id: 10");
                var received1 = await TimeoutAfter(Requests.RequestMessage(connection, 10), TimeSpan.FromSeconds(2));
                Console.WriteLine("Received message: ID: [{0}], Description: [{1}], Updated: [{2}]", received1.Id, received1.Description, received1.Updated);
            }
            catch (TimeoutException e)
            {
                Console.WriteLine(e.Message);
            }
            connection.Close();
            Console.WriteLine("Disconnected");
        }
        private static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using var timeoutCancellationTokenSource = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
            if (completedTask == task)
            {
                timeoutCancellationTokenSource.Cancel();
                return await task;
            } 
            else
            {
                throw new TimeoutException("The operation has timed out");
            }
        }
    }
}