﻿using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Transport.Messaging;
using NLightning.Utils.Extensions;

namespace NLightning.Network.GossipMessages
{
    public class ChannelAnnouncementMessage : Message
    {
        public static readonly MessageDefinition MessageDefinition = new MessageDefinition(256, typeof(ChannelAnnouncementMessage),
            new List<Property> {
                new Property("Node Signature 1", PropertyTypes.Signature),
                new Property("Node Signature 2", PropertyTypes.Signature),
                new Property("Bitcoin Signature 1", PropertyTypes.Signature),
                new Property("Bitcoin Signature 2", PropertyTypes.Signature),
                new Property("Features", PropertyTypes.VariableArray),
                new Property("Chain Hash", PropertyTypes.Hash32),
                new Property("Short Channel ID", PropertyTypes.ShortChannelId),
                new Property("Node ID 1", PropertyTypes.PublicKey),
                new Property("Node ID 2", PropertyTypes.PublicKey),
                new Property("Bitcoin Key 1", PropertyTypes.PublicKey),
                new Property("Bitcoin Key 2", PropertyTypes.PublicKey)
            }.AsReadOnly());
        
        public ChannelAnnouncementMessage() : base(MessageDefinition)
        {
        }
        
        public ChannelAnnouncementMessage(byte[] nodeSignature1, byte[] nodeSignature2, byte[] bitcoinSignature1, byte[] bitcoinSignature2, 
                                          byte[] features, byte[] chainHash, string shortChannelIdHex, byte[] shortChannelId, string nodeId1Hex, 
                                          string nodeId2Hex, ECKeyPair bitcoinKey1, ECKeyPair bitcoinKey2) : base(MessageDefinition)
        {
            NodeSignature1 = nodeSignature1;
            NodeSignature2 = nodeSignature2;
            BitcoinSignature1 = bitcoinSignature1;
            BitcoinSignature2 = bitcoinSignature2;
            Features = features;
            ChainHash = chainHash;
            ShortChannelIdHex = shortChannelIdHex;
            ShortChannelId = shortChannelId;
            NodeId1Hex = nodeId1Hex;
            NodeId2Hex = nodeId2Hex;
            NodeId1 = new ECKeyPair(nodeId1Hex, false);
            NodeId2 = new ECKeyPair(nodeId2Hex, false);
            BitcoinKey1 = bitcoinKey1;
            BitcoinKey2 = bitcoinKey2;
        }
        
        public byte[] NodeSignature1 { get; private set; }
        public byte[] NodeSignature2 { get; private set; }
        public byte[] BitcoinSignature1 { get; private set; }
        public byte[] BitcoinSignature2 { get; private set; }
        public byte[] Features { get; private set;}
        public byte[] ChainHash { get; private set;}
        public string ShortChannelIdHex { get; private set; }
        public byte[] ShortChannelId { get; private set; }
        public string NodeId1Hex { get; private set; }
        public string NodeId2Hex { get; private set; }
        public ECKeyPair NodeId1 { get; private set; }
        public ECKeyPair NodeId2 { get; private set; }
        public ECKeyPair BitcoinKey1 { get; private set; }
        public ECKeyPair BitcoinKey2 { get; private set; }
       
        public override void SetProperties(List<byte[]> propertyData)
        {
            NodeSignature1 = propertyData[0];
            NodeSignature2 = propertyData[1];
            BitcoinSignature1 = propertyData[2];
            BitcoinSignature2 = propertyData[3];
            Features = propertyData[4];
            ChainHash = propertyData[5];
            ShortChannelId = propertyData[6];
            ShortChannelIdHex = propertyData[6].ToHex();
            NodeId1Hex = propertyData[7].ToHex();
            NodeId2Hex = propertyData[8].ToHex();
            NodeId1 = new ECKeyPair(propertyData[7], false);
            NodeId2 = new ECKeyPair(propertyData[8], false);
            BitcoinKey1 = new ECKeyPair(propertyData[9], false);
            BitcoinKey2 = new ECKeyPair(propertyData[10], false);
        }

        public override List<byte[]> GetProperties()
        {
            return new List<byte[]>
            {
                NodeSignature1,
                NodeSignature2,
                BitcoinSignature1,
                BitcoinSignature2,
                Features,
                ChainHash,
                ShortChannelId,
                NodeId1.PublicKeyCompressed,
                NodeId2.PublicKeyCompressed,
                BitcoinKey1.PublicKeyCompressed,
                BitcoinKey2.PublicKeyCompressed
            };
        }
    }
}