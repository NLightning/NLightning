using NLightning.Cryptography;
using NLightning.OnChain;
using NLightning.Peer;
using NLightning.Transport.Messaging;
using NLightning.Transport.Messaging.Validation;
using NLightning.Utils.Extensions;

namespace NLightning.Network.GossipMessages
{
    public class NodeAnnouncementMessageValidator : MessageValidator<NodeAnnouncementMessage>
    {
        protected override void ValidateMessage(NodeAnnouncementMessage message, byte[] rawData)
        {
            var witness = SHA256.ComputeHash(SHA256.ComputeHash(rawData.SubArray(66, rawData.Length - 66)));
            if (!Secp256K1.VerifySignature(witness, message.Signature, message.NodeId))
            {
                throw new MessageValidationException(message, "NodeAnnouncementMessage: Invalid Signature", true);
            }
        }
    }
}