using System;

namespace NLightning.Network
{
    public interface INetworkViewSyncService
    {
        bool Synchronised { get; } 
        float SyncProgressPercentage { get; } 
        IObservable<float> SyncProgressPercentageProvider { get; }
        
        void Initialize(NetworkParameters networkParameters);
    }
}