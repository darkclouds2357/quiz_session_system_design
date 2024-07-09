

using Asp.Versioning;

namespace NotifierWorker.Controllers
{
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v1/channels")]
    public class ChannelController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ChannelController> _logger;

        public ChannelController(IMediator mediator, ILogger<ChannelController> logger)
        {
            this._mediator = mediator;
            this._logger = logger;
        }


    }
}
