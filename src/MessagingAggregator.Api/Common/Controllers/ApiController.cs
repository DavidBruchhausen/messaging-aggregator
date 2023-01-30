using System.Net;
using MessagingAggregator.Api.Common.Responses;
using MessagingAggregator.Api.Common.Responses.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace MessagingAggregator.Api.Common.BaseController;

public class ApiController : ControllerBase
{
    protected IActionResult Ok(string message = null)
    {
        var meta = new Meta(HttpStatusCode.OK, message);
        return base.Ok(new MetaResponse<Meta>(meta));
    }

    protected IActionResult Ok<TData>(TData data, string message = null)
    {
        var meta = new Meta(HttpStatusCode.OK, message);
        return base.Ok(new DataResponse<Meta, TData>(meta, data));
    }

    protected IActionResult Created<TData>(TData data, string message = null)
    {
        var meta = new Meta(HttpStatusCode.Created, message);
        return base.Created(string.Empty, new DataResponse<Meta, TData>(meta, data));
    }
}
