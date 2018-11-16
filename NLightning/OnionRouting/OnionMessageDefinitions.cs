using System.Collections.Generic;
using System.Collections.ObjectModel;
using NLightning.OnionRouting.OnionFailureMessages;

namespace NLightning.OnionRouting
{
    public static class OnionMessageDefinitions
    {
        public static readonly ReadOnlyCollection<OnionMessageDefinition> Definitions = new ReadOnlyCollection<OnionMessageDefinition>(
            new List<OnionMessageDefinition>
            {
                AmountBelowMinimumMessage.MessageDefinition,
                ChannelDisabledMessage.MessageDefinition,
                ExpiryTooFarMessage.MessageDefinition,
                ExpiryTooSoonMessage.MessageDefinition,
                FeeInsufficientMessage.MessageDefinition,
                FinalExpiryTooSoonMessage.MessageDefinition,
                FinalIncorrectCltvExpiryMessage.MessageDefinition,
                FinalIncorrectHtlcAmountMessage.MessageDefinition,
                IncorrectCltvExpiryMessage.MessageDefinition,
                IncorrectPaymentAmountMessage.MessageDefinition,
                InvalidOnionHmacMessage.MessageDefinition,
                InvalidOnionKeyMessage.MessageDefinition,
                InvalidOnionVersionMessage.MessageDefinition,
                InvalidRealmMessage.MessageDefinition,
                PermanentChannelFailureMessage.MessageDefinition,
                PermanentNodeFailureMessage.MessageDefinition,
                RequiredChannelFeatureMissingMessage.MessageDefinition,
                RequiredNodeFeatureMissingMessage.MessageDefinition,
                TemporaryChannelFailureMessage.MessageDefinition,
                TemporaryNodeFailureMessage.MessageDefinition,
                UnknownNextPeerMessage.MessageDefinition,
                UnknownPaymentHashMessage.MessageDefinition
            });
    }
}