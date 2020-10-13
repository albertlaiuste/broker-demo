using System.Threading.Tasks;

namespace BrokerDemo.Client.Broker
{
    public static class Requests
    {
        public static Task<API.DemoMessage> RequestMessage(BrokerConnection connection, int id)
        {
            API.DemoMessage message = null;
            var tcs = new TaskCompletionSource<API.DemoMessage>();

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