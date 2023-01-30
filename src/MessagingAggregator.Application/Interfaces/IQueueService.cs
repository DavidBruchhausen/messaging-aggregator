using MessagingAggregator.Application.Models;

namespace MessagingAggregator.Application.Interfaces;

public interface IQueueService
{
    void SendMessage(Message model);
}