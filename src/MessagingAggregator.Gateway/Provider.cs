using System.Text;
using Microsoft.Extensions.Configuration;
using MessagingAggregator.Application.Interfaces;
using MessagingAggregator.Application.Common.Extensions;
using MessagingAggregator.Gateway.Responses;
using Serilog;

namespace MessagingAggregator.Gateway;

public class Provider : IProvider
{
    private readonly HttpClient _client;
    private readonly ILogger _logger = Log.ForContext<Provider>();

    public Provider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        IHttpClientFactory _httpClientFactory = httpClientFactory;
        _client = _httpClientFactory.CreateClient();
        _client.BaseAddress = new Uri(configuration.GetValue<string>("AGGREGATED_MESSAGE_SERVER_URL"));
    }

    public async Task SendPostRequest(object body, string route)
    {
        var json = body.AsJson();

        Console.WriteLine(_client.BaseAddress);
        var data = json.FormatAsHttpContent(Encoding.UTF8, "application/json");
        var httpResponse = await _client.PostAsync(route, data);
        var content = await httpResponse.Content.ReadAsStringAsync();

        HandleResponse(httpResponse, content);
    }

    private void HandleResponse(HttpResponseMessage httpResponse, string content)
    {
        if (httpResponse.IsSuccessStatusCode) return;

        try
        {
            ErrorResponse errorResponse = content.FromJson<ErrorResponse>();
            _logger.Error("Error response: {errorResponse}", errorResponse.Message);
        }
        catch
        {
            throw new Exception(content);
        }
    }
}