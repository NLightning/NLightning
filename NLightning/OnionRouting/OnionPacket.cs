using System.Collections.Generic;
using NLightning.Cryptography;

namespace NLightning.OnionRouting
{
    public class OnionPacket
    {
        public OnionPacket(ECKeyPair ephemeralKey, byte[] hMac, List<PerHopData> hopsData, byte version, byte[] data)
        {
            EphemeralKey = ephemeralKey;
            HMac = hMac;
            HopsData = hopsData;
            Version = version;
            Data = data;
        }

        public byte Version { get; }
        public ECKeyPair EphemeralKey { get; }
        public List<PerHopData> HopsData { get; }
        public byte[] Data { get; }
        public byte[] HMac { get; }
    }
}