using System;

namespace NLightning.Network.Models
{
    public class PeerNetworkViewState
    {
        public string PeerNetworkAddress { get; set; }
        public DateTime LastUpdated { get; set; }
        public uint LastBlockNumber { get; set; } = 1;

        public void UpdateBlockNumber(uint blockNumber)
        {
            if (blockNumber < LastBlockNumber)
            {
                return;
            }

            LastBlockNumber = blockNumber;
            LastUpdated = DateTime.Now;
        }

        public void ResetBlockNumber()
        {
            LastBlockNumber = 0;
            LastUpdated = DateTime.Now;
        }
    }
}