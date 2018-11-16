using System;
using Microsoft.Extensions.Logging;
using NLightning.Transport;

namespace NLightning.Utils.Extensions
{
    public static class LoggerFactoryExtensions
    {
        public static ILogger CreateNodeAddressLogger(this ILoggerFactory factory, Type type, NodeAddress remoteAddress)
        {
            ILogger logger = factory.CreateLogger($"{type.Name}, Peer: {remoteAddress}");
            return logger;
        }
    }
}