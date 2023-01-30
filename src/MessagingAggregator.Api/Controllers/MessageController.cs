using Microsoft.AspNetCore.Mvc;
using MessagingAggregator.Application.Models;
using MessagingAggregator.Application.Interfaces;
using MessagingAggregator.Api.Common.BaseController;

namespace MessagingAggregator.Api.Controllers
{
    public class MessageController : ApiController
    {
        private readonly IQueueService _queueService;

        public MessageController(IQueueService queueService)
        {
            _queueService = queueService;
        }

        [HttpPost("message")]
        public IActionResult CaptureMessage([FromBody] Message request)
        {
            _queueService.SendMessage(request);
            return Ok();
        }
    }
}