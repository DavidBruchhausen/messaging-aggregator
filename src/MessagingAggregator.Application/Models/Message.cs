namespace MessagingAggregator.Application.Models;

public class Message
{
    public string Destination { get; set; }
    public string Text { get; set; }
    public DateTime Timestamp { get; set; }
}