namespace BrokerDemo.Server.Broker
{
    public interface IBrokerConnection
    {
        void Open();
        void Close();
        void PublishRead(string queueName, API.DemoMessage message);
        void SubscribeWrite();
        void SubscribeRead();
    }
}