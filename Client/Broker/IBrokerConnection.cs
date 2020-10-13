using System;

namespace Client.Broker
{
    public interface IBrokerConnection
    {
        void Open();
        void Close();
        void Produce(BrokerDemo.API.DemoMessage message);
        void Request(int id);
        void Subscribe();
    }
}