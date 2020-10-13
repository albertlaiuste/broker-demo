using System.Threading.Tasks;

namespace Client.Broker
{
    public static class Requests
    {
        public static Task<BrokerDemo.API.DemoMessage> RequestMessage(BrokerConnection connection, int id)
        {
            BrokerDemo.API.DemoMessage message = null;
            var tcs = new TaskCompletionSource<BrokerDemo.API.DemoMessage>();

            BrokerConnection.MessageReceivedEventHandler handler = null;
            handler = (o, e) =>
            {
                message = e.Message;
                connection.MessageReceivedEvent -= handler;
                tcs.SetResult(message);
            };
            connection.MessageReceivedEvent += handler;
            connection.Request(id);
            
            return tcs.Task;
        }
    }
}