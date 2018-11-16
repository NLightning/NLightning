using System.Collections;

namespace NLightning.Network.Models
{
    public class ChannelFlags
    {
        public bool Disabled { get; private set; }
        public bool Node1IsOriginator { get; private set; }

        private ChannelFlags()
        {
        }

        public static ChannelFlags Parse(byte data)
        {
            ChannelFlags flags = new ChannelFlags();
            BitArray bitArray = new BitArray(new byte[] { data });

            flags.Node1IsOriginator = !bitArray.Get(0);
            flags.Disabled = bitArray.Get(1);
            
            return flags;
        }
        
    }
}