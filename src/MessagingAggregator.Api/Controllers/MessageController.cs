using Microsoft.AspNetCore.Mvc;
using MessagingAggregator.Application.Models;
using MessagingAggregator.Application.Interfaces;
using MessagingAggregator.Api.Common.BaseController;
using Swashbuckle.AspNetCore.Annotations;
using MessagingAggregator.Api.Common.Responses;
using MessagingAggregator.Api.Common.Responses.Metadata;
using MessagingAggregator.Application.Common.Exceptions;

namespace MessagingAggregator.Api.Controllers;

[ApiController]
public class MessageController : ApiController
{
    private readonly IQueueService _queueService;

    public MessageController(IQueueService queueService)
    {
        _queueService = queueService;
    }

    [HttpPost("message")]
    [ProducesResponseType(typeof(MetaResponse<Meta>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MetaResponse<Meta>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MetaResponse<Meta>), StatusCodes.Status500InternalServerError)]
    public IActionResult CaptureMessage([FromBody] Message request)
    {
        _queueService.SendMessage(request);
        return Ok("Message recevied.");
    }
}