using System;

namespace NLightning.Network.Configuration
{
    public class NetworkViewConfiguration
    {
        public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan ChannelTimeout { get; set; } = TimeSpan.FromDays(14);
        public TimeSpan NodeTimeout { get; set; } = TimeSpan.FromDays(14);
        public TimeSpan SynchronisationTimeout { get; set; } = TimeSpan.FromSeconds(60);
        public SynchronisationMode SynchronisationMode { get; set; } = SynchronisationMode.Automatic;
    }
}