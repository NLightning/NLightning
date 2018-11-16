using System.Collections.Generic;
using System.Linq;
using NLightning.Network.Models;

namespace NLightning.Network.Routing
{
    public class SimpleRouter : IRouter
    {
        private readonly int _maxHops;
        private readonly int _maxRoutes;
        
        public SimpleRouter(int maxHops = 4, int maxRoutes = 16)
        {
            _maxHops = maxHops;
            _maxRoutes = maxRoutes;
        }
        
        public List<Route> FindRoutes(NetworkNode senderNode, NetworkChannel senderNetworkChannel, NetworkChannel receiverNetworkChannel)
        {
            Stack<NetworkChannel> channelStack = new Stack<NetworkChannel>();
            List<Route> routes = new List<Route>();
            NetworkNode nextNode = senderNetworkChannel.Node1 == senderNode ? senderNetworkChannel.Node2 : senderNetworkChannel.Node1;
            
            channelStack.Push(senderNetworkChannel);
            
            for(int depth = 2; depth < _maxHops + 2 && routes.Count < _maxRoutes; depth ++)
            {
                FindRoutes(nextNode, channelStack, routes, receiverNetworkChannel, depth);
            }

            return routes.OrderBy(r => r.ChannelPath.Count).ToList();
        }

        private void FindRoutes(NetworkNode currentNode, 
            Stack<NetworkChannel> channelStack, List<Route> routesFound, NetworkChannel receiverNetworkChannel, int depth)
        {
            var nextChannels = currentNode.GetAllChannels().ToList();
            if (depth == channelStack.Count && nextChannels.Contains(receiverNetworkChannel))
            {
                var path = channelStack.ToList();
                path.Add(receiverNetworkChannel);
                routesFound.Add(new Route(path));
                return;
            }

            if (channelStack.Count < depth)
            {
                foreach (var nextChannel in nextChannels)
                {
                    var nextNode = nextChannel.Node1 == currentNode ? nextChannel.Node2 : nextChannel.Node1;

                    channelStack.Push(nextChannel);
                    FindRoutes(nextNode, channelStack, routesFound, receiverNetworkChannel, depth);
                    channelStack.Pop();

                    if (routesFound.Count >= _maxRoutes)
                    {
                        return;
                    }
                }
            }
        }
    }
}