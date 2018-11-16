using NLightning.Peer;
using Xunit;

namespace NLightning.Test.Peer
{
    public class PeerFeaturesTest
    {
        [Fact]
        public void NoFeaturesTest()
        {
            PeerFeatures localfeatures = PeerFeatures.Parse(new byte[] { 0 });
            
            Assert.False(localfeatures.CompulsoryDataLossProtection);
            Assert.False(localfeatures.OptionalGossipQueries);
            Assert.False(localfeatures.OptionalInitialRoutingSync);
            Assert.False(localfeatures.CompulsoryUpfrontShutdownScript);
            Assert.False(localfeatures.CompulsoryGossipQueries);
            Assert.False(localfeatures.OptionalDataLossProtection);
            Assert.False(localfeatures.OptionalUpfrontShutdownScript);
        }

        [Fact]
        public void ParseDataLossProtectionTest()
        {
            PeerFeatures localfeatures = PeerFeatures.Parse(new byte[] { 1 });
            
            Assert.True(localfeatures.CompulsoryDataLossProtection);
            Assert.False(localfeatures.OptionalGossipQueries);
            Assert.False(localfeatures.OptionalInitialRoutingSync);
            Assert.False(localfeatures.CompulsoryUpfrontShutdownScript);
            Assert.False(localfeatures.CompulsoryGossipQueries);
            Assert.False(localfeatures.OptionalDataLossProtection);
            Assert.False(localfeatures.OptionalUpfrontShutdownScript);
        }
        
        [Fact]
        public void ParseDataLossProtection2Test()
        {
            PeerFeatures localfeatures = PeerFeatures.Parse(new byte[] { 3 });
            
            Assert.True(localfeatures.CompulsoryDataLossProtection);
            Assert.True(localfeatures.OptionalDataLossProtection);
            Assert.False(localfeatures.OptionalGossipQueries);
            Assert.False(localfeatures.OptionalInitialRoutingSync);
            Assert.False(localfeatures.CompulsoryUpfrontShutdownScript);
            Assert.False(localfeatures.CompulsoryGossipQueries);
            Assert.False(localfeatures.OptionalUpfrontShutdownScript);
        }
        
        [Fact]
        public void ParseGossipQueriesTest()
        {
            PeerFeatures peerFeatures = new PeerFeatures();
            peerFeatures.OptionalGossipQueries = true;
            
            PeerFeatures parsedFeatures = PeerFeatures.Parse(peerFeatures.GetBytes());
            
            Assert.True(parsedFeatures.OptionalGossipQueries);
            
            Assert.False(parsedFeatures.CompulsoryDataLossProtection);
            Assert.False(parsedFeatures.OptionalInitialRoutingSync);
            Assert.False(parsedFeatures.CompulsoryUpfrontShutdownScript);
            Assert.False(parsedFeatures.CompulsoryGossipQueries);
            Assert.False(parsedFeatures.OptionalDataLossProtection);
            Assert.False(parsedFeatures.OptionalUpfrontShutdownScript);
        }
        
        [Fact]
        public void ParseGossipQueries2Test()
        {
            PeerFeatures peerFeatures = new PeerFeatures();
            peerFeatures.OptionalGossipQueries = true;
            peerFeatures.CompulsoryGossipQueries = true;
            
            PeerFeatures parsedFeatures = PeerFeatures.Parse(peerFeatures.GetBytes());
            
            Assert.True(parsedFeatures.CompulsoryGossipQueries);
            Assert.True(parsedFeatures.OptionalGossipQueries);
            Assert.False(parsedFeatures.CompulsoryDataLossProtection);
            Assert.False(parsedFeatures.OptionalDataLossProtection);
            Assert.False(parsedFeatures.OptionalInitialRoutingSync);
            Assert.False(parsedFeatures.CompulsoryUpfrontShutdownScript);
            Assert.False(parsedFeatures.OptionalUpfrontShutdownScript);
        }
       
        [Fact]
        public void ParseAllSetTest()
        {
            PeerFeatures peerFeatures = new PeerFeatures();
            peerFeatures.OptionalGossipQueries = true;
            peerFeatures.CompulsoryGossipQueries = true;
            peerFeatures.CompulsoryDataLossProtection = true;
            peerFeatures.CompulsoryUpfrontShutdownScript = true;
            peerFeatures.OptionalDataLossProtection = true;
            peerFeatures.OptionalInitialRoutingSync = true;
            peerFeatures.OptionalUpfrontShutdownScript = true;
            
            PeerFeatures localfeatures = PeerFeatures.Parse(peerFeatures.GetBytes());
            
            Assert.True(localfeatures.CompulsoryGossipQueries);
            Assert.True(localfeatures.OptionalGossipQueries);
            Assert.True(localfeatures.OptionalInitialRoutingSync);
            Assert.True(localfeatures.CompulsoryUpfrontShutdownScript);
            Assert.True(localfeatures.OptionalDataLossProtection);
            Assert.True(localfeatures.OptionalUpfrontShutdownScript);
        }
        
        [Fact]
        public void ToStringTest()
        {
            PeerFeatures localfeatures = PeerFeatures.Parse(new byte[] { 255 });
            Assert.Equal("PeerFeatures: Compulsory Data Loss Protection, Optional Data Loss Protection, Optional Initial Routing Sync, Compulsory Upfront Shutdown Script, Optional Upfront Shutdown Scripts, Compulsory Gossip Queries, Optional Gossip Queries", localfeatures.ToString());
            
            localfeatures = PeerFeatures.Parse(new byte[] { 1 });
            Assert.Equal("PeerFeatures: Compulsory Data Loss Protection", localfeatures.ToString());

            localfeatures = PeerFeatures.Parse(new byte[] { 0 });
            Assert.Equal("PeerFeatures: none", localfeatures.ToString());
        }
    }
}