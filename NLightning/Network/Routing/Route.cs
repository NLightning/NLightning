using System;
using System.Collections.Generic;
using System.Linq;
using NLightning.Network.Models;

namespace NLightning.Network.Routing
{
    public class Route
    {
        public Route(List<NetworkChannel> channelPath)
        {
            ChannelPath = channelPath;
        }

        public List<NetworkChannel> ChannelPath { get; private set; }

        public List<NetworkNode> NodePath
        {
            get
            {
                List<NetworkNode> nodes = new List<NetworkNode>();

                for (var i = 0; i < ChannelPath.Count; i++)
                {
                    bool node1IsNext;
                    
                    if (i + 1 < ChannelPath.Count)
                    {
                        node1IsNext = ChannelPath[i].Node1 == ChannelPath[i + 1].Node1 || ChannelPath[i].Node1 == ChannelPath[i + 1].Node2;
                    }
                    else
                    {
                        node1IsNext = ChannelPath[i].Node1 != ChannelPath[i - 1].Node1 && ChannelPath[i].Node1 != ChannelPath[i - 1].Node2;
                    }
                    
                    nodes.Add(node1IsNext ? ChannelPath[0].Node2 : ChannelPath[0].Node1);
                }
                
                return nodes;
            }
        }

        public NetworkChannel SenderNetworkChannel => ChannelPath.First();
        public NetworkChannel ReceiverNetworkChannel => ChannelPath.Last();
    }
}