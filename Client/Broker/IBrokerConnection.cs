namespace BrokerDemo.Client.Broker
{
    public interface IBrokerConnection
    {
        void Open();
        void Close();
        void Produce(API.DemoMessage message);
        void Request(int id);
        void Subscribe();
    }
}