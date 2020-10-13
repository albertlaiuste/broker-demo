using System;

namespace Client.Broker
{
    public interface IBrokerConnection
    {
        void Open();
        void Close();
        void Produce(int id, string description, DateTime timestamp);
        void Request(int id);
        void Subscribe();
    }
}