using System;
using NLightning.Peer.Channel.Models;

namespace NLightning.Peer.Channel
{
    public class ChannelException : Exception
    {
        public LocalChannel Channel { get; }

        public ChannelException(string message, LocalChannel channel = null, Exception innerException = null) : base(message, innerException)
        {
            Channel = channel;
        }
    }
}