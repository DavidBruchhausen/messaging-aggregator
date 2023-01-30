using MessagingAggregator.Api.Common.Responses.Metadata;

namespace MessagingAggregator.Api.Common.Responses;

public class DataResponse<TMeta, TData> : MetaResponse<TMeta>
    where TMeta : Meta
{
    public TData Data { get; }

    public DataResponse(TMeta meta, TData data) : base(meta)
    {
        Data = data;
    }
}
