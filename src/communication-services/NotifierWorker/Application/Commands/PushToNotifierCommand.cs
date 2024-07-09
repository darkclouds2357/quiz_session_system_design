
using Newtonsoft.Json;
using System.Text;

namespace NotifierWorker.Application.Commands
{
    public class PushToNotifierCommand : IRequest
    {
        public string Channel { get; set; }
        public object Payload { get; set; }
        public string EventName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PushToNotifierCommandHandler : IRequestHandler<PushToNotifierCommand>
    {
        private readonly ILogger<PushToNotifierCommandHandler> _logger;

        public PushToNotifierCommandHandler(ILogger<PushToNotifierCommandHandler> logger)
        {
            this._logger = logger;
        }

        public Task Handle(PushToNotifierCommand request, CancellationToken cancellationToken)
        {

            _logger.LogInformation(@"
======================================================================================================
This Log for simulate action after handle push command send Data to client.
1. Event message created from source at: {createdAt}
2. Convert channel: [{channel}] to private/group connection that user register when Subscribe to server
3. Push data payload of event [{eventName}] to client: 
---------
{payload}
---------
4. In case that user lost connect, it can call to communication server to get current state of private/group channel 
*for firebase realtime database, just call to firebase realtime database to get current state of channel
======================================================================================================
", request.CreatedAt, request.Channel, request.EventName, JsonConvert.SerializeObject(request.Payload));

            return Task.CompletedTask;
        }
    }
}
