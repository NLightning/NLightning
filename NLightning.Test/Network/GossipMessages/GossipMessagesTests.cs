using System;
using System.Collections.Generic;
using NLightning.Cryptography;
using NLightning.Network.Address;
using NLightning.Network.GossipMessages;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Network.GossipMessages
{
    public class GossipMessagesTests
    {
        [Fact]
        public void AnnouncementSignaturesMessageTest()
        {
            var message = new AnnouncementSignaturesMessage()
            {
                ChannelId = "b34fafd163cf765b0997187c20c8fe52dde14d8edd0b4406428e882930bb0723".HexToByteArray(),
                ShortChannelId = "b34fafd163cf765b".HexToByteArray(),
                NodeSignature = "310e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464456452".HexToByteArray(),
                BitcoinSignature = "410e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464456452".HexToByteArray()
            };
            
            Assert.Equal(259, message.Definition.TypeId);
            
            Assert.Equal("b34fafd163cf765b0997187c20c8fe52dde14d8edd0b4406428e882930bb0723", message.ChannelId.ToHex());
            Assert.Equal("b34fafd163cf765b", message.ShortChannelId.ToHex());
            Assert.Equal("310e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464456452", message.NodeSignature.ToHex());
            Assert.Equal("410e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464456452", message.BitcoinSignature.ToHex());
            
            var properties = message.GetProperties();
            
            Assert.Equal("b34fafd163cf765b0997187c20c8fe52dde14d8edd0b4406428e882930bb0723", properties[0].ToHex());
            Assert.Equal("b34fafd163cf765b", properties[1].ToHex());
            Assert.Equal("310e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464456452", properties[2].ToHex());
            Assert.Equal("410e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464456452", properties[3].ToHex());

            Assert.Equal("0103b34fafd163cf765b0997187c20c8fe52dde14d8edd0b4406428e882930bb0723b34fafd163cf765b310e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464456452410e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464456452",
                            message.GetBytes().ToHex());
        }
        
        [Fact]
        public void ChannelAnnouncementMessageTest()
        {
            var message = new ChannelAnnouncementMessage(
                "111e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464451111".HexToByteArray(),
                "222e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464452222".HexToByteArray(),
                "333e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464453333".HexToByteArray(),
                "444e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464454444".HexToByteArray(),
                "52".HexToByteArray(),
                "6fe28c0ab6f1b372c1a6a246ae63f74f931e8365e15a089c68d6190000000000".HexToByteArray(),
                "b34fafd163cf765b",
                "b34fafd163cf765b".HexToByteArray(),
                "023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb",
                "0212a140cd0c6539d07cd08dfe09984dec3251ea808b892efeac3ede9402bf2b19",
                new ECKeyPair("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa", false), 
                new ECKeyPair("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991", false)
            );
            
            Assert.Equal(256, message.Definition.TypeId);
            
            Assert.Equal("111e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464451111", message.NodeSignature1.ToHex());
            Assert.Equal("222e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464452222", message.NodeSignature2.ToHex());
            Assert.Equal("333e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464453333", message.BitcoinSignature1.ToHex());
            Assert.Equal("444e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464454444", message.BitcoinSignature2.ToHex());
            Assert.Equal("52", message.Features.ToHex());
            Assert.Equal("6fe28c0ab6f1b372c1a6a246ae63f74f931e8365e15a089c68d6190000000000", message.ChainHash.ToHex());
            Assert.Equal("b34fafd163cf765b", message.ShortChannelId.ToHex());
            Assert.Equal("b34fafd163cf765b", message.ShortChannelIdHex);
            Assert.Equal("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb", message.NodeId1Hex);
            Assert.Equal("0212a140cd0c6539d07cd08dfe09984dec3251ea808b892efeac3ede9402bf2b19", message.NodeId2Hex);
            Assert.Equal("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb", message.NodeId1.PublicKeyCompressed.ToHex());
            Assert.Equal("0212a140cd0c6539d07cd08dfe09984dec3251ea808b892efeac3ede9402bf2b19", message.NodeId2.PublicKeyCompressed.ToHex());
            Assert.Equal("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa", message.BitcoinKey1.PublicKeyCompressed.ToHex());
            Assert.Equal("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991", message.BitcoinKey2.PublicKeyCompressed.ToHex());
            
            var properties = message.GetProperties();
            
            Assert.Equal("111e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464451111", properties[0].ToHex());
            Assert.Equal("222e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464452222", properties[1].ToHex());
            Assert.Equal("333e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464453333", properties[2].ToHex());
            Assert.Equal("444e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464454444", properties[3].ToHex());
            Assert.Equal("52", properties[4].ToHex());
            Assert.Equal("6fe28c0ab6f1b372c1a6a246ae63f74f931e8365e15a089c68d6190000000000", properties[5].ToHex());
            Assert.Equal("b34fafd163cf765b", properties[6].ToHex());
            Assert.Equal("023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb", properties[7].ToHex());
            Assert.Equal("0212a140cd0c6539d07cd08dfe09984dec3251ea808b892efeac3ede9402bf2b19", properties[8].ToHex());
            Assert.Equal("034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa", properties[9].ToHex());
            Assert.Equal("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991", properties[10].ToHex());
            
            
            Assert.Equal("0100111e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464451111" +
                        "222e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464452222" +
                        "333e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464453333" +
                        "444e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464454444" + 
                        "000152" +
                        "6fe28c0ab6f1b372c1a6a246ae63f74f931e8365e15a089c68d6190000000000" + 
                        "b34fafd163cf765b" +
                        "023da092f6980e58d2c037173180e9a465476026ee50f96695963e8efe436f54eb" +
                        "0212a140cd0c6539d07cd08dfe09984dec3251ea808b892efeac3ede9402bf2b19" +
                        "034f355bdcb7cc0af728ef3cceb9615d90684bb5b2ca5f859ab0f0b704075871aa" +
                        "032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991", message.GetBytes().ToHex());
        }
        
        [Fact]
        public void ChannelUpdateMessageTest()
        {
            var message = new ChannelUpdateMessage();
            
            message.SetProperties(new List<byte[]>
            {
                "111e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464451111".HexToByteArray(),
                "6fe28c0ab6f1b372c1a6a246ae63f74f931e8365e15a089c68d6190000000000".HexToByteArray(),
                "b34fafd163cf765b".HexToByteArray(),
                "11111111".HexToByteArray(),
                new byte[] {1},
                new byte[] {2},
                "2211".HexToByteArray(),
                "3333666611113333".HexToByteArray(),
                "55558888".HexToByteArray(),
                "66669999".HexToByteArray()
            });
            
            Assert.Equal(258, message.Definition.TypeId);
            
            Assert.Equal("111e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464451111", message.Signature.ToHex());
            Assert.Equal("6fe28c0ab6f1b372c1a6a246ae63f74f931e8365e15a089c68d6190000000000", message.ChainHash.ToHex());
            Assert.Equal("b34fafd163cf765b", message.ShortChannelId.ToHex());
            Assert.Equal(DateTime.Parse("1979-01-28T01:25:53.0000000+01:00"), message.Timestamp);
            Assert.Equal(1, message.MessageFlags);
            Assert.Equal(2, message.ChannelFlags);
            Assert.Equal(8721, message.CltvExpiryDelta);
            Assert.Equal((ulong)3689405108305605427, message.HtlcMinimumSat);
            Assert.Equal((ulong)1431668872, message.FeeBaseMsat);
            Assert.Equal((ulong)1718000025, message.FeeProportionalMillionths);
            
            var properties = message.GetProperties();
            
            Assert.Equal("111e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464451111", properties[0].ToHex());
            Assert.Equal("6fe28c0ab6f1b372c1a6a246ae63f74f931e8365e15a089c68d6190000000000", properties[1].ToHex());
            Assert.Equal("b34fafd163cf765b", properties[2].ToHex());
            Assert.Equal("11111111", properties[3].ToHex());
            Assert.Equal("01", properties[4].ToHex());
            Assert.Equal("02", properties[5].ToHex());
            Assert.Equal("2211", properties[6].ToHex());
            Assert.Equal("3333666611113333", properties[7].ToHex());
            Assert.Equal("55558888", properties[8].ToHex());
            Assert.Equal("66669999", properties[9].ToHex());
            
            Assert.Equal("0102"+
                         "111e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464451111" +
                         "6fe28c0ab6f1b372c1a6a246ae63f74f931e8365e15a089c68d6190000000000" +
                         "b34fafd163cf765b" +
                         "11111111" +
                         "01" +
                         "02" +
                         "2211" +
                         "3333666611113333" +
                         "55558888" +
                         "66669999",
                message.GetBytes().ToHex());
        }
        
                
        [Fact]
        public void NodeAnnouncementMessageTest()
        {
            List<NetworkAddress> addresses = new List<NetworkAddress>();
            addresses.Add(new NetworkAddress(AddressType.IpV4, "10.11.22.33", 6677));
            
            var message = new NodeAnnouncementMessage(
                "111e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464451111".HexToByteArray(),
                "1122".HexToByteArray(),
                DateTime.Parse("1979-01-28T01:25:53.0000000+01:00"),
                new ECKeyPair("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991", false),
                "223366",
                "my test node",
                addresses);
            
            Assert.Equal(257, message.Definition.TypeId);
            
            Assert.Equal("111e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464451111", message.Signature.ToHex());
            Assert.Equal("1122", message.Features.ToHex());
            Assert.Equal(DateTime.Parse("1979-01-28T01:25:53.0000000+01:00"), message.Timestamp);
            Assert.Equal("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991", message.NodeId.PublicKeyCompressed.ToHex());
            Assert.Equal("223366", message.Color);
            Assert.Equal("my test node", message.Alias);
            Assert.Single(message.GetAddresses());
            Assert.Equal("10.11.22.33", message.GetAddresses()[0].Address);
            Assert.Equal(6677, message.GetAddresses()[0].Port);
            Assert.Equal(AddressType.IpV4, message.GetAddresses()[0].Type);

            var properties = message.GetProperties();
            
            Assert.Equal("111e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464451111", properties[0].ToHex());
            Assert.Equal("1122", properties[1].ToHex());
            Assert.Equal("11111111", properties[2].ToHex());
            Assert.Equal("032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991", properties[3].ToHex());
            Assert.Equal("223366", properties[4].ToHex());
            Assert.Equal("6d792074657374206e6f64652020202020202020202020202020202020202020", properties[5].ToHex());
            Assert.Equal("010a0b16211a15", properties[6].ToHex());
            
            var address = NetworkAddress.Decode("010a0b16211a15".HexToByteArray())[0];
            Assert.Equal("10.11.22.33", address.Address);
            Assert.Equal(6677, address.Port);
            Assert.Equal(AddressType.IpV4, address.Type);
            
            Assert.Equal("0101" +
                        "111e45454b0978a623f36a10626ef17b27d9ad44e2760f98cfa3efb37924f0220220bd8acd43ecaa916a80bd4f919c495a2c58982ce7c8625112456464451111" +
                        "00021122" +
                        "11111111" +
                        "032c0b7cf95324a07d05398b240174dc0c2be444d96b159aa6c7f7b1e668680991" + 
                        "223366" +
                        "6d792074657374206e6f64652020202020202020202020202020202020202020" + 
                        "0007010a0b16211a15", message.GetBytes().ToHex());
        }
    }
}