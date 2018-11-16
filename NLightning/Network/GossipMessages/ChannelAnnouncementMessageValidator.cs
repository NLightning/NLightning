using System;
using System.Linq;
using NLightning.Cryptography;
using NLightning.OnChain;
using NLightning.Peer;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;

namespace NLightning.Network.GossipMessages
{
    public class ChannelAnnouncementMessageValidator : MessageValidator<ChannelAnnouncementMessage>
    {
        private NetworkParameters _networkParameters;

        protected override void ValidateMessage(ChannelAnnouncementMessage message, byte[] rawData)
        {
            var witness = SHA256.ComputeHash(SHA256.ComputeHash(rawData.SubArray(258, rawData.Length - 258)));
            
            if (!Secp256K1.VerifySignature(witness, message.NodeSignature1, message.NodeId1))
            {
                throw new MessageValidationException(message, "ChannelAnnouncementMessage: Invalid Signature (Node 1)", true);
            }
            
            if (!Secp256K1.VerifySignature(witness, message.NodeSignature2, message.NodeId2))
            {
                throw new MessageValidationException(message, "ChannelAnnouncementMessage: Invalid Signature (Node 2)", true);
            }
            
            if (!Secp256K1.VerifySignature(witness, message.BitcoinSignature1, message.BitcoinKey1))
            {
                throw new MessageValidationException(message, "ChannelAnnouncementMessage: Invalid Signature (Bitcoin Key 1)", true);
            }
            
            if (!Secp256K1.VerifySignature(witness, message.BitcoinSignature2, message.BitcoinKey2))
            {
                throw new MessageValidationException(message, "ChannelAnnouncementMessage: Invalid Signature (Bitcoin Key 2)", true);
            }
               
            if (!message.ChainHash.SequenceEqual(_networkParameters.ChainHash))
            {
                throw new MessageValidationException(message, "ChannelAnnouncementMessage: Invalid chain hash (ChannelAnnouncementMessage)");
            }

        }
        
        public void Initialize(NetworkParameters networkParameters)
        {
            _networkParameters = networkParameters;
        }
    }
}