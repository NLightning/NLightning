﻿using System;
using System.Collections.Generic;
using System.Text;
using NLightning.Cryptography;
using NLightning.Network.Address;
using NLightning.Transport.Messaging;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using Message = NLightning.Transport.Messaging.Message;

namespace NLightning.Network.GossipMessages
{
    public class NodeAnnouncementMessage : Message
    {
        private byte[] _networkAddresses;
        
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(257, typeof(NodeAnnouncementMessage),
            new List<Property> {
                new Property("Signature", PropertyTypes.Signature),
                new Property("Features", PropertyTypes.VariableArray),
                new Property("Timestamp", PropertyTypes.Timestamp),
                new Property("Node ID", PropertyTypes.PublicKey),
                new Property("Color", PropertyTypes.Color),
                new Property("Alias", PropertyTypes.Alias),
                new Property("Addresses", PropertyTypes.VariableArray)
            }.AsReadOnly());
        
        public NodeAnnouncementMessage() : base(MessageDefinition)
        {
        }
       
        public NodeAnnouncementMessage(byte[] signature, byte[] features, DateTime timestamp, ECKeyPair nodeId,
                                        String color, String alias, List<NetworkAddress> addresses) : base(MessageDefinition)
        {
            Signature = signature;
            Features = features;
            Timestamp = timestamp;
            NodeId = nodeId;
            Color = color;
            Alias = alias;
            _networkAddresses = NetworkAddress.Encode(addresses);
            NodeIdHex = nodeId.PublicKeyCompressed.ToHex();
        }
        
        public byte[] Signature { get; private set; }
        public byte[] Features { get; private set;}
        public DateTime Timestamp { get; private set;}
        public ECKeyPair NodeId { get; private set; }
        public string NodeIdHex { get; private set; }
        public string Color { get; private set;}
        public string Alias { get; private set;}

        public string AliasSanitized
        {
            get
            {
                var alias = Alias.PadRight(32);

                if (alias.Length > 32)
                {
                    return alias.Substring(0, 32);
                }

                return alias;
            }
        }


        public List<NetworkAddress> GetAddresses() => NetworkAddress.Decode(_networkAddresses);
        
        public override void SetProperties(List<byte[]> propertyData)
        {
            Signature = propertyData[0];
            Features = propertyData[1];
            Timestamp = DateTimeExtensions.CreateFromUnixSeconds(propertyData[2].ToUIntBigEndian());
            NodeId = new ECKeyPair(propertyData[3], false);
            NodeIdHex = propertyData[3].ToHex();
            Color = propertyData[4].ToHex();
            Alias = Encoding.ASCII.GetString(propertyData[5]);
            _networkAddresses = propertyData[6];
        }

        public override List<byte[]> GetProperties()
        {
            byte[] alias = new byte[32];
            byte[] aliasSource = Encoding.ASCII.GetBytes(AliasSanitized);
            Array.Copy(aliasSource, 0, alias, 0, 32);
            
            return new List<byte[]> { 
                Signature, Features, 
                Timestamp.ToUnixSeconds().GetBytesBigEndian(),
                NodeId.PublicKeyCompressed,
                Color.HexToByteArray(),
                alias,
                _networkAddresses
            };
        }
    }
}