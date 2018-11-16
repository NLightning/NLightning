using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NLightning.Peer
{
    public class PeerFeatures
    {
        public bool CompulsoryDataLossProtection { get; set; }
        public bool OptionalDataLossProtection { get; set; }
        public bool OptionalInitialRoutingSync { get; set; }
        public bool CompulsoryUpfrontShutdownScript { get;set; }
        public bool OptionalUpfrontShutdownScript { get;set; }
        public bool CompulsoryGossipQueries { get; set; }
        public bool OptionalGossipQueries { get; set; }

        public PeerFeatures()
        {
        }
        
        public PeerFeatures(bool compulsoryDataLossProtection, bool optionalDataLossProtection,
                            bool optionalOptionalInitialRoutingSync, 
                            bool compulsoryUpfrontShutdownScript, bool optionalUpfrontShutdownScript,
                            bool compulsoryGossipQueries, bool optionalGossipQueries)
        {
            CompulsoryDataLossProtection = compulsoryDataLossProtection;
            OptionalDataLossProtection = optionalDataLossProtection;
            OptionalInitialRoutingSync = optionalOptionalInitialRoutingSync;
            CompulsoryUpfrontShutdownScript = compulsoryUpfrontShutdownScript;
            OptionalUpfrontShutdownScript = optionalUpfrontShutdownScript;
            CompulsoryGossipQueries = compulsoryGossipQueries;
            OptionalGossipQueries = optionalGossipQueries;
        }

        public byte[] GetBytes()
        {
            byte[] data = new byte[1];
            BitArray bitArray = new BitArray(data);

            if (CompulsoryDataLossProtection)
            {
                bitArray.Set(0, true);
            }
            
            if (OptionalDataLossProtection)
            {
                bitArray.Set(1, true);
            }
            
            if (OptionalInitialRoutingSync)
            {
                bitArray.Set(3, true);
            }
            
            if (CompulsoryUpfrontShutdownScript)
            {
                bitArray.Set(4, true);
            }
            
            if (OptionalUpfrontShutdownScript)
            {
                bitArray.Set(5, true);
            }
            
            if (CompulsoryGossipQueries)
            {
                bitArray.Set(6, true);
            }
            
            if (OptionalGossipQueries)
            {
                bitArray.Set(7, true);
            }
            
            bitArray.CopyTo(data, 0);
                        
            return data;
        }

        public static PeerFeatures Parse(byte[] data)
        {
            if (data.Length == 0)
            {
                return new PeerFeatures();
            }
            
            BitArray bitArray = new BitArray(data);
            return new PeerFeatures(bitArray.Get(0), bitArray.Get(1), 
                                     bitArray.Get(3), 
                                     bitArray.Get(4), bitArray.Get(5), 
                                     bitArray.Get(6), bitArray.Get(7));
        }

        public override String ToString()
        {
            List<String> features = new List<string>();
            
            if (CompulsoryDataLossProtection)
            {
                features.Add("Compulsory Data Loss Protection");
            }
            
            if (OptionalDataLossProtection)
            {
                features.Add("Optional Data Loss Protection");
            }
            
            if (OptionalInitialRoutingSync)
            {
                features.Add("Optional Initial Routing Sync");
            }
            
            if (CompulsoryUpfrontShutdownScript)
            {
                features.Add("Compulsory Upfront Shutdown Script");
            }
            
            if (OptionalUpfrontShutdownScript)
            {
                features.Add("Optional Upfront Shutdown Scripts");
            }
            
            if (CompulsoryGossipQueries)
            {
                features.Add("Compulsory Gossip Queries");
            }
            
            if (OptionalGossipQueries)
            {
                features.Add("Optional Gossip Queries");
            }

            if (features.Count == 0)
            {
                return "PeerFeatures: none";
            }
            
            return "PeerFeatures: " + features.Aggregate((f1, f2) => $"{f1}, {f2}");
        }
    }
}