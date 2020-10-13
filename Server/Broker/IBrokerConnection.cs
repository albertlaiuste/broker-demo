using System;
using BrokerDemo.API;

namespace Server.Broker
{
    public interface IBrokerConnection
    {
        void Open();
        void Close();
        void PublishRead(string queueName, BrokerDemo.API.DemoMessage message);
        void SubscribeWrite();
        void SubscribeRead();
    }
}