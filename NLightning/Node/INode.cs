using System;
using Microsoft.Extensions.DependencyInjection;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.OnChain;

namespace NLightning.Node
{
    public interface INode
    {
        IServiceProvider Services { get; }
        ECKeyPair LocalKey { get; }
        NetworkParameters NetworkParameters { get; }
        void ConfigureServices(Action<IServiceCollection> configureServices);
    }
}