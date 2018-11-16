using System;
using System.Reactive.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLightning.Bitcoind;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.Node;
using NLightning.OnChain.Client;
using NLightning.Peer;
using NLightning.Peer.Channel;
using NLightning.Persistence;
using NLightning.Transport;

namespace NLightning.Console
{
    class Program
    {
        static void MyMain(string[] args)
        {
            string localPrivateKey = "<node private key>";
            ECKeyPair localLightningKey = new ECKeyPair(localPrivateKey, true);
            
            string walletSeed = "<wallet private key>";
            ECKeyPair walletKey = new ECKeyPair(walletSeed, true);

            LightningNode node = new LightningNode(localLightningKey, walletKey, NetworkParameters.BitcoinTestnet);
            
            node.ConfigureServices(services =>
            {
                services.AddSingleton<IBlockchainClientService, BitcoindClientService>();
                services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Debug)
                                                      .AddConsole(options => options.IncludeScopes = false)
                                                      .AddDebug()
                                                      .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning));
                
                services.AddDbContext<NetworkPersistenceContext>(options => options.UseSqlite("Data Source=network.db"));
                services.AddDbContext<LocalPersistenceContext>(options => options.UseSqlite("Data Source=local.db"));
            });

            node.Initialize();
            
            // Peer Service: 
            var peerService = node.Services.GetService<IPeerService>();
            
            // Channel Service:
            var channels = node.Services.GetService<IChannelService>();
            
            // Channel Establishment Service:
            var channelEstablishmentService = node.Services.GetService<IChannelEstablishmentService>();
            
            // Channel State Service:
            var channelStateService = node.Services.GetService<IChannelStateService>();
            
            // Channel Close Service
            var closeService = node.Services.GetService<IChannelCloseService>();
            
            channelEstablishmentService.FailureProvider.Subscribe(peerAndPendingChannel =>
            {
                // Failed to open a channel
            });
            
            channelEstablishmentService.SuccessProvider.Subscribe(peerAndPendingChannel =>
            {
                // Opened a channel
            });

            channelStateService.ChannelActiveStateChangedProvider
                .Where(c => c.Active)
                .Subscribe(channel =>
                {
                    // Channel is active
                });
            
            NodeAddress nodeAddress = NodeAddress.Parse("<pubkey@ip:port>");
            
            // Connect to node:
            IPeer peer = peerService.AddPeer(nodeAddress, persist: false, reconnect: true);
            
            // Open a channel with 25000 satoshis capacity
            channelEstablishmentService.OpenChannel(peer, 25000);

            System.Console.ReadKey();
        }
    }
}