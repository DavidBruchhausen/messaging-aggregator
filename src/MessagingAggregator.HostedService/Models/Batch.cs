namespace MessagingAggregator.HostedService.Models;

public class Batch
{
    public string Destination { get; set; }
    public List<MessageDto> Messages { get; set; }
}