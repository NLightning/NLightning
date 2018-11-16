using System;
using Microsoft.EntityFrameworkCore;
using NLightning.Network.Models;

namespace NLightning.Persistence
{
    public interface IPersistenceContext : IDisposable
    {
        DbSet<NetworkChannel> NetworkChannels { get; set; }
        DbSet<NetworkNode> Nodes { get; set; }
    }
}