using System;
using System.Text;
using NBitcoin;
using NLightning.Utils.Extensions;

namespace NLightning.Transport.Messaging
{
    public class PropertyTypes
    {
        public static readonly PropertyType Signature = new PropertyType("Signature", typeof(byte[]), 64);
        public static readonly PropertyType Signatures = new PropertyType("Signatures", typeof(byte[])) { LengthPerProperty = 64 };
        public static readonly PropertyType VariableArray = new PropertyType("Variable Array", typeof(byte[]));
        public static readonly PropertyType ChannelId = new PropertyType("Channel ID", typeof(byte[]), 32);
        public static readonly PropertyType TransactionId = new PropertyType("Transaction ID", typeof(byte[]), 32, bytes => $"{uint256.Parse(bytes.ToHex())} (0x{bytes.ToHex()})");
        public static readonly PropertyType ShortChannelId = new PropertyType("Short Channel ID", typeof(byte[]), 8);
        
        public static readonly PropertyType Byte = new PropertyType("Byte", typeof(byte), 1);
        public static readonly PropertyType UShort = new PropertyType("Unsigned Short", typeof(ushort), 2, bytes => $"{bytes.ToUShortBigEndian()} (0x{bytes.ToHex()})");
        public static readonly PropertyType UInt = new PropertyType("Unsigned Integer", typeof(uint), 4, bytes => $"{bytes.ToUIntBigEndian()} (0x{bytes.ToHex()})");
        public static readonly PropertyType ULong = new PropertyType("Unsigned Long", typeof(ulong), 8, bytes => $"{bytes.ToULongBigEndian()} (0x{bytes.ToHex()})");
        public static readonly PropertyType Hash32 = new PropertyType("Hash (32 Bytes)", typeof(byte[]), 32);
        public static readonly PropertyType RoutingPacket1366 = new PropertyType("Routing Packet (1366 Bytes)", typeof(byte[]), 1366);
        
        public static readonly PropertyType PublicKey = new PropertyType("Public Key", typeof(byte[]), 33);
        public static readonly PropertyType PrivateKey = new PropertyType("Private Key", typeof(byte[]), 32);
        public static readonly PropertyType Timestamp = new PropertyType("Timestamp", typeof(int), 4, 
            bytes => $"{DateTimeExtensions.CreateFromUnixSeconds(bytes.ToUIntBigEndian())} (0x{bytes.ToHex()})");
        
        public static readonly PropertyType Color = new PropertyType("Color", typeof(byte[]), 3);
        public static readonly PropertyType Alias = new PropertyType("Alias", typeof(String), 32, bytes => $"{Encoding.ASCII.GetString(bytes)} (0x{bytes.ToHex()})");
        public static readonly PropertyType Flags2 = new PropertyType("Flags (2 Bytes)", typeof(byte[]), 2);
    }
}