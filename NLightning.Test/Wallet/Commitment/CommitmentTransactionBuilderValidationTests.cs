using NLightning.Cryptography;
using NLightning.Peer.Channel.Models;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using NLightning.Wallet.Commitment;
using NLightning.Wallet.Commitment.Models;
using NLightning.Wallet.KeyDerivation;
using Xunit;

namespace NLightning.Test.Wallet.Commitment
{
    public class CommitmentTransactionValidationTests
    {
        [Fact]
        public void ValidateTest()
        {
            CommitmentTransactionBuilder builder = new CommitmentTransactionBuilder();

/*
 *          OpenChannelMessage(32)
            Chain Hash (Hash (32 Bytes), 32):            43497fd7f826957108f4a30fd9cec3aeba79972084e90ead01ea330900000000
            Temporary Channel ID (Channel ID, 32):       9b33024da3d0f725434cbaa91dfbdcd18d8e372d879d2a0c94d5e23836786715
            Funding Satoshis (Unsigned Long, 8):         25000 (0x00000000000061a8)
            Push mSat (Unsigned Long, 8):                0 (0x0000000000000000)
            Dust Limit Satoshis (Unsigned Long, 8):      546 (0x0000000000000222)
            Max HTLC Value In Flight mSat (Unsigned Long, 8): 5000000000 (0x000000012a05f200)
            Channel Reserve Satoshis (Unsigned Long, 8): 2500 (0x00000000000009c4)
            HTLC Minimum mSat (Unsigned Long, 8):        1000 (0x00000000000003e8)
            Feerate Per KW (Unsigned Integer, 4):        1011 (0x000003f3)
            To Self Delay (Unsigned Short, 2):           144 (0x0090)
            Max Accepted HTLCs (Unsigned Short, 2):      483 (0x01e3)
            Funding PubKey (Public Key, 33):             0250d049da6b5832a9f2416df3b0db52da127426c2b70a35ca9c270a72f3f840b5
            Revocation Basepoint (Public Key, 33):       022ecc432552ff86d053514ffb133d3025fb14c39aa5ae2a5169b0367174cabfa4
            Payment Basepoint (Public Key, 33):          029d100efe40aa3f58985fa12bd0f5c75711449ff4d30adca6f1968a2200bbbf1a
            Delayed Payment Basepoint (Public Key, 33):  0245b02f6672c2342fe3ced57118fcf4a0309327e32c335ce494365eb0d15b7200
            HTLC Basepoint (Public Key, 33):             03d029229db8f594adcd545b4a42acbb1013286908d2905fa05c9a4e2083fe3fe2
            First Per Commitment Point (Public Key, 33): 02846726efa57378ad8370acf094f26902a7f1e21903791ef4ab6f989da86679f2
            Channel Flags (Byte, 1):                     00
            Shutdown ScriptPubKey (Variable Array, 25):  76a9141c60596620b0b9966400cb710b8da6de5a80d68588ac

            AcceptChannelMessage(33)
            Temporary Channel ID (Channel ID, 32):       9b33024da3d0f725434cbaa91dfbdcd18d8e372d879d2a0c94d5e23836786715
            Dust Limit Satoshis (Unsigned Long, 8):      573 (0x000000000000023d)
            Max HTLC Value In Flight mSat (Unsigned Long, 8): 24750000 (0x000000000179a7b0)
            Channel Reserve Satoshis (Unsigned Long, 8): 546 (0x0000000000000222)
            HTLC Minimum mSat (Unsigned Long, 8):        1000 (0x00000000000003e8)
            Minimum Depth (Unsigned Integer, 4):         3 (0x00000003)
            To Self Delay (Unsigned Short, 2):           144 (0x0090)
            Max Accepted HTLCs (Unsigned Short, 2):      483 (0x01e3)
            Funding PubKey (Public Key, 33):             0299de4bbf495e5bbeb2456c2beb3f40450a3fa41aaa50819ae201f8ad69226bfe
            Revocation Basepoint (Public Key, 33):       022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92
            Payment Basepoint (Public Key, 33):          02d91224d91760f477df21d24713b713c681b084e508f48dc77ca14db549ba8ceb
            Delayed Payment Basepoint (Public Key, 33):  0341665cedb568e09f0ab2ab4a28bc2749620deacefb3dce61aac8251c91709d3a
            HTLC Basepoint (Public Key, 33):             0336439e36e2bc1f264c6d3bc6e12db6256389bef2056c32e6267d6e285c2b2122
            First Per Commitment Point (Public Key, 33): 039360132ab07e7f56d6782a644233da9c4c24845609fcd302cbedd69f69848358

            FundingCreatedMessage(34)
            Temporary Channel ID (Channel ID, 32):       9b33024da3d0f725434cbaa91dfbdcd18d8e372d879d2a0c94d5e23836786715
            Funding Transaction ID (Transaction ID, 32): 282ea2263611611169ee505fc83979ecaf1ad99b565f9d64c1b1fb804c427da3
            Funding Output Index (Unsigned Short, 2):    1 (0x0001)
            Signature (Signature, 64):                   4fed2d370e0166934d63d4b0ca5c523839ca12bc2acdc5c3238c33a1460e409d39a5d267d7132abe3b8c6bf46c2bf456a47b4462931558af371dfd1f7312f0a5
     
            FundingSignedMessage(35)
            Channel ID (Channel ID, 32):                 282ea2263611611169ee505fc83979ecaf1ad99b565f9d64c1b1fb804c427da2
            Signature (Signature, 64):                   2f26e967305b4d422116a7c876d338bc4298263f329a54c0d1655f55d594de4955356386addd0bc15ced36dacece0af439694195654c838769583409eeac5d3f
       
 */
            
            // Local
            ECKeyPair fundingKeyPrivateKey =         new ECKeyPair("DD06232AE9A50384A72D85CED6351DCB35C798231D4985615C77D6847F83FC65", true);
            ECKeyPair localFundingPubKey =           new ECKeyPair("0250d049da6b5832a9f2416df3b0db52da127426c2b70a35ca9c270a72f3f840b5", false);
            ECKeyPair localPaymentBasepoint =        new ECKeyPair("029d100efe40aa3f58985fa12bd0f5c75711449ff4d30adca6f1968a2200bbbf1a", false);
            ECKeyPair localDelayedPaymentBasepoint = new ECKeyPair("0245b02f6672c2342fe3ced57118fcf4a0309327e32c335ce494365eb0d15b7200", false);
            ECKeyPair localPerCommitmentPoint =      new ECKeyPair("02846726efa57378ad8370acf094f26902a7f1e21903791ef4ab6f989da86679f2", false);
            
            // Remote
            ECKeyPair remoteRevocationBasepoint =    new ECKeyPair("022b2aa486f5a8aca1898824ac3b2a8a15c92de813362846b992f94d923b143f92", false);
            ECKeyPair remotePaymentBasepoint =       new ECKeyPair("02d91224d91760f477df21d24713b713c681b084e508f48dc77ca14db549ba8ceb", false);
            ECKeyPair remoteFundingKey =             new ECKeyPair("0299de4bbf495e5bbeb2456c2beb3f40450a3fa41aaa50819ae201f8ad69226bfe", false);
            
            // Derive Local
            RevocationPublicKeyDerivation revocationPublicKeyDerivation = new RevocationPublicKeyDerivation(localPerCommitmentPoint);
            PublicKeyDerivation publicKeyDerivation = new PublicKeyDerivation(localPerCommitmentPoint);
            
            ECKeyPair localDelayedPaymentPubkey = publicKeyDerivation.Derive(localDelayedPaymentBasepoint);
            ECKeyPair localRevocationPubkey = revocationPublicKeyDerivation.DerivePublicKey(remoteRevocationBasepoint);
            
            builder.CommitmentTxParams = new CommitmentTransactionParameters();
            builder.CommitmentTxParams.TransactionNumber = 0;
            builder.CommitmentTxParams.RevocationPublicKey = localRevocationPubkey;
            builder.CommitmentTxParams.DelayedPaymentPublicKey = localDelayedPaymentPubkey;
            builder.CommitmentTxParams.PaymentBasepoint = localPaymentBasepoint;
            builder.CommitmentTxParams.FundingKey = localFundingPubKey;
            builder.FeeratePerKw = 1011;             
            builder.CommitmentTxParams.ToLocalMsat = 25000000;
            builder.CommitmentTxParams.ToRemoteMsat = 0;
            builder.FundingTransactionOutputIndex = 1;
            builder.FundingAmount = 25000;
            builder.FundingTransactionHash = "a37d424c80fbb1c1649d5f569bd91aafec7939c85f50ee691161113626a22e28";
            builder.IsFunder = true;
            builder.ChannelParameters = new ChannelParameters();
            builder.ChannelParameters.DustLimitSatoshis = 546;
            builder.ChannelParameters.ToSelfDelay = 144;
            builder.RemotePaymentBasePoint = remotePaymentBasepoint;
            builder.RemoteFundingPubKey = remoteFundingKey;
            builder.Network = NBitcoin.Network.TestNet;
            
            var signature = SignatureConverter.RawToTransactionSignature("2f26e967305b4d422116a7c876d338bc4298263f329a54c0d1655f55d594de4955356386addd0bc15ced36dacece0af439694195654c838769583409eeac5d3f"
                .HexToByteArray());
           
            Assert.True(builder.IsValidSignature(signature, fundingKeyPrivateKey));
        }
    }
}