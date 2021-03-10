using System;

namespace AirportHandling
{
    public interface IMessageQueueClient : IDisposable
    {
        void PublishToQueue<TDto>(string queue, TDto dto);
        void Subscribe<TDto>(string queue, Action<TDto> onMessageCallback);
    }
}
