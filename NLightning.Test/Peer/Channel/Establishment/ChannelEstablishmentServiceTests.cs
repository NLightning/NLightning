using System.Collections.Generic;
using System.Linq;
using Moq;
using NBitcoin;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.Peer.Channel.Establishment.Messages;
using NLightning.Peer.Channel.Models;
using NLightning.Utils.Extensions;
using NLightning.Wallet;
using Xunit;

namespace NLightning.Test.Peer.Channel.Establishment
{
    public class ChannelEstablishmentServiceTests
    {
        [Fact]
        public void InitializeTest()
        {
            var mocks = new ChannelEstablishmentMocks();
            var channel = mocks.CreateChannelMock();

            channel.State = LocalChannelState.FundingLocked;
            mocks.SetupMocks();
            mocks.ChannelService.Setup(c => c.Channels)
                .Returns(() => new List<LocalChannel>() {channel}.AsReadOnly());

            var service = mocks.CreateServiceMock();
            service.Initialize(NetworkParameters.BitcoinTestnet);            
            mocks.BlockchainMonitorService
                .Verify(bms => bms.WatchForTransactionId(
                    It.Is<string>(id => id == channel.FundingTransactionId), 
                    It.Is<ushort>(id => id == channel.MinimumDepth)), 
                    Times.Once());
        }

        [Fact]
        public void OpenChannelTest()
        {
            var mocks = new ChannelEstablishmentMocks();
            mocks.SetupMocks();

            var revocationKey = new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
            var walletKey = new Key("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65".HexToByteArray());
            
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.DelayBase, 0)).Returns(new ECKeyPair("0250d049da6b5832a9f2416df3b0db52da127426c2b70a35ca9c270a72f3f840b5"));
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.HtlcBase, 0)).Returns(new ECKeyPair("029d100efe40aa3f58985fa12bd0f5c75711449ff4d30adca6f1968a2200bbbf1a"));
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.MultiSig, 0)).Returns(new ECKeyPair("0245b02f6672c2342fe3ced57118fcf4a0309327e32c335ce494365eb0d15b7200"));
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.NodeKey, 0)).Returns(new ECKeyPair("02846726efa57378ad8370acf094f26902a7f1e21903791ef4ab6f989da86679f2"));
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.PaymentBase, 0)).Returns(new ECKeyPair("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92"));
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.RevocationBase, 0)).Returns(new ECKeyPair("02d91224d91760f477df21d24713b713c681b084e508f48dc77ca14db549ba8ceb"));
            mocks.KeyDerivationService.Setup(kds => kds.DeriveKey(KeyFamily.RevocationRoot, 0)).Returns(revocationKey);
            mocks.KeyDerivationService.Setup(kds => kds.DerivePerCommitmentPoint(It.Is<ECKeyPair>(key => key.PrivateKeyData.SequenceEqual(revocationKey.PrivateKeyData)), 0))
                .Returns(new ECKeyPair("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92"));

            mocks.WalletService.Setup(ws => ws.ShutdownScriptPubKey).Returns(walletKey.PubKey.GetAddress(NBitcoin.Network.TestNet).ScriptPubKey.ToBytes);
            
            var service = mocks.CreateServiceMock();
            service.Initialize(NetworkParameters.BitcoinTestnet);
            var pendingChannel = service.OpenChannel(mocks.Peer.Object, 42000, 100);
            
            mocks.MessagingClient
                .Verify(mock => mock.Send(
                    It.Is<OpenChannelMessage>(message => VerifyOpenMessage(message))), Times.Once());
            
            mocks.ChannelService.Verify(mock => mock.AddPendingChannel(pendingChannel), Times.Once());
        }

        private bool VerifyOpenMessage(OpenChannelMessage message)
        {
            Assert.Equal(0, message.ChannelFlags);
            Assert.Equal((ulong)42000, message.FundingSatoshis);
            Assert.Equal((ulong)4200, message.ChannelReserveSatoshis);
            Assert.Equal((ulong)546, message.DustLimitSatoshis);
            Assert.Equal((ulong)253, message.FeeratePerKw);
            Assert.Equal((ulong)483, message.MaxAcceptedHtlcs);
            Assert.Equal((ulong)100, message.PushMSat);
            Assert.Equal((ulong)144, message.ToSelfDelay);
            Assert.Equal((ulong)1000, message.HtlcMinimumMSat);
            Assert.Equal((ulong)5000000000, message.MaxHtlcValueInFlightMSat);
            
            Assert.Equal("43497fd7f826957108f4a30fd9cec3aeba79972084e90ead01ea330900000000".HexToByteArray(), message.ChainHash);
            Assert.Equal("029d100efe40aa3f58985fa12bd0f5c75711449ff4d30adca6f1968a2200bbbf1a", message.HtlcBasepoint.PublicKeyCompressed.ToHex());
            Assert.Equal("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92", message.PaymentBasepoint.PublicKeyCompressed.ToHex());
            Assert.Equal("02d91224d91760f477df21d24713b713c681b084e508f48dc77ca14db549ba8ceb", message.RevocationBasepoint.PublicKeyCompressed.ToHex());
            Assert.Equal("0250d049da6b5832a9f2416df3b0db52da127426c2b70a35ca9c270a72f3f840b5", message.DelayedPaymentBasepoint.PublicKeyCompressed.ToHex());
            Assert.Equal("0245b02f6672c2342fe3ced57118fcf4a0309327e32c335ce494365eb0d15b7200", message.FundingPubKey.PublicKeyCompressed.ToHex());
            Assert.Equal("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92", message.FirstPerCommitmentPoint.PublicKeyCompressed.ToHex());
            Assert.Equal("76a914f75d1c854c52abee075916b41cf0ca76fa515b4c88ac", message.ShutdownScriptPubKey.ToHex());
            
            return true;
        }
    }
}