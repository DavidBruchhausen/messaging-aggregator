using System.Net;

namespace MessagingAggregator.Api.Common.Responses.Metadata;

public class Meta
{
    public string Status { get; set; }
    public string Message { get; set; }

    public Meta(HttpStatusCode statusCode, string message)
    {
        Status = Convert.ToString(statusCode);
        Message = message;
    }
}
