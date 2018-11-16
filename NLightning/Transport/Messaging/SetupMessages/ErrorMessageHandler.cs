using Microsoft.Extensions.Logging;
using NLightning.Utils;
using NLightning.Utils.Extensions;

namespace NLightning.Transport.Messaging.SetupMessages
{
    public class ErrorMessageHandler : MessageHandler<ErrorMessage>
    {
        private readonly NodeAddress _nodeAddress;
        private ILogger _logger;

        public ErrorMessageHandler(ILoggerFactory loggerFactory, NodeAddress nodeAddress)
        {
            _logger = loggerFactory.CreateNodeAddressLogger(GetType(), nodeAddress);
            _nodeAddress = nodeAddress;
        }

        protected override void HandleMessage(ErrorMessage message)
        {
            _logger.LogError("Error Message received: " + message.Data);
        }

        public override void Dispose()
        {
        }
    }
}