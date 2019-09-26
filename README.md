# NLightning

<p>NLightning is a library to connect, synchronize, send and receive payments with the lightning network.</p>
<p>Current implementation uses bitcoind (with -txindex) as backend.</p>

__⚠ NLightning is in an early development stage, do not use with real funds!__

Features:
- [x] DNS bootstrap 
- [x] Download and synchronize network view 
- [x] Open channels
- [x] Mutual and unilateral closing of channels
- [ ] Handle penalty and remote unilateral close 
- [ ] Send and receive payments
- [ ] SPV wallet support (BIP 157, BIP 158)
- [ ] Backup channels
- [ ] Support watchtowers

Supported platforms:
- .NET Core (Linux, MacOS, Windows)

Not yet supported: 
- Xamarin iOS
- Xamarin Android

Dependencies:
- NBitcoin
- EFCore
- Portable.BouncyCastle
- System.Reactive
- Microsoft Extensions (Logging)
- DnsClient
- xUnit, Moq


## Usage

### Configuration

```C#
ECKeyPair localLightningKey = new ECKeyPair("<node private key>", true);
ECKeyPair walletKey = new ECKeyPair("<wallet private key>", true);

LightningNode node = new LightningNode(localLightningKey, walletKey, NetworkParameters.BitcoinTestnet);

node.ConfigureServices(services =>
{
    // by default NLightning loads the configuration from a nlightning.json file located in the working directory.
    // if you want to load your own configuration instead add it to the services:
    // services.AddSingleton(new ConfigurationBuilder()
    //                       .SetBasePath(Directory.GetCurrentDirectory())
    //                       .AddJsonFile("my-config.json", true, true));
        
    // we use bitcoind (with -txindex) as backend
    services.AddSingleton<IBlockchainClientService, BitcoindClientService>();
    
    // configure logging
    services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Debug)
                                          .AddConsole(options => options.IncludeScopes = false)
                                          .AddDebug()
                                          .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning));
    
    // configure network db (gossip data)
    // we support sqlite, but others should work as well
    services.AddDbContext<NetworkPersistenceContext>(options => options.UseSqlite("Data Source=network.db"));
    
    // configure local db (channel and peer data) 
    // private keys are stored here
    services.AddDbContext<LocalPersistenceContext>(options => options.UseSqlite("Data Source=local.db"));
});

// Start all services.
node.Initialize();
```

Configuration Entities: `BitcoindClientConfiguration`, `BlockchainConfiguration`, `ChannelConfiguration`, `NetworkViewConfiguration`, `PeerConfiguration`

### Connect to a node

```C#
IPeerService peerService = node.Services.GetService<IPeerService>();
NodeAddress nodeAddress = NodeAddress.Parse("<publickey@ip:port>");
IPeer peer = peerService.AddPeer(nodeAddress, persist: true, reconnect: true);
```

### Open a channel

```C#
var channelEstablishmentService = node.Services.GetService<IChannelEstablishmentService>();
channelEstablishmentService.SuccessProvider.Subscribe(peerAndChannel =>
{
    // $"Opened a channel (ID: {peerAndChannel.Channel.ChannelId} with peer {peerAndChannel.Peer.NodeAddress}";  
});

// Open a channel with a funding of 25000 Satoshis
channelEstablishmentService.OpenChannel(peer, 25000);
```

### Get all active channels and updates

```C#
IChannelService channelService = node.Services.GetService<IChannelService>();
var activeChannels = channelService.Channels.Where(c => c.Active);
```

```C#
var channelStateService = node.Services.GetService<IChannelStateService>();
channelStateService.ChannelActiveStateChangedProvider
    .Where(c => c.Active)
    .Subscribe(channel =>
    {
        // Channel is active
    });
```

### Close a channel

```C#
IChannelCloseService closeService = node.Services.GetService<IChannelCloseService>();

// mutual close, if peer is not available do an unilateral close:
closeService.Close(channel, unilateralCloseOnUnavailability: true);

// unilateral close:
closeService.UnilateralClose(channel);
```

### Get all channels and nodes in the network

```C#
var networkViewService = node.Services.GetService<INetworkViewService>();
var networkChannels = networkViewService.View.GetChannels();
var networkNodes = networkViewService.View.GetNodes();
```
