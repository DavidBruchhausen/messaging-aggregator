using System.Text;
using System.Text.Json;

namespace MessagingAggregator.Application.Common.Extensions;

public static class TypeExtensions
{
    public static string AsJson(this object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    public static T FromJson<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public static StringContent FormatAsHttpContent(this string content, Encoding encoding, string mediaType)
    {
        return new StringContent(content, encoding, mediaType);
    }
}