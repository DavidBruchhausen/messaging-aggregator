using MessagingAggregator.Api.Common.Responses.Metadata;

namespace MessagingAggregator.Api.Common.Responses;

public class MetaResponse
{

}

public class MetaResponse<TMeta>
    where TMeta : Meta
{
    public TMeta Meta { get; }

    public MetaResponse(TMeta meta)
    {
        Meta = meta;
    }
}
