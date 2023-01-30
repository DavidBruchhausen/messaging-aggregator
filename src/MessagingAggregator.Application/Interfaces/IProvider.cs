namespace MessagingAggregator.Application.Interfaces;

public interface IProvider
{
    Task SendPostRequest(object body, string route);
}