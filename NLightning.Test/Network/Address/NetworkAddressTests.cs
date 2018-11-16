using System.Collections.Generic;
using NLightning.Network.Address;
using Xunit;
using NetworkAddress = NLightning.Network.Address.NetworkAddress;

namespace NLightning.Test.Network.Address
{
    public class NetworkAddressTests
    {
        [Fact]
        public void EncodeDecodeTest()
        {
            NetworkAddress address1 = new NetworkAddress(AddressType.IpV4, "1.2.3.4", 1234);

            byte[] encoded = NetworkAddress.Encode(new List<NetworkAddress> {address1, address1});
            var decoded = NetworkAddress.Decode(encoded);
            
            Assert.Equal("1.2.3.4", decoded[0].Address);
            Assert.Equal((ushort)1234, decoded[0].Port);
        }
        
        [Fact]
        public void EncodeDecodeTest2()
        {
            NetworkAddress address1 = new NetworkAddress(AddressType.IpV4, "1.2.3.4", 1234);
            NetworkAddress address2 = new NetworkAddress(AddressType.IpV6, "2001:db8:85a3:8d3:1319:8a2e:370:7348", 5678);
            NetworkAddress address3 = new NetworkAddress(AddressType.IpV4, "5.0.7.255", ushort.MaxValue);
            NetworkAddress address4 = new NetworkAddress(AddressType.IpV6, "fe80::1ff:fe23:4567:890a", 0);
            
            byte[] encoded = NetworkAddress.Encode(new List<NetworkAddress> {address1, address2, address3, address4});
            var decoded = NetworkAddress.Decode(encoded);
            
            Assert.Equal("1.2.3.4", decoded[0].Address);
            Assert.Equal((ushort)1234, decoded[0].Port);
            
            Assert.Equal("2001:db8:85a3:8d3:1319:8a2e:370:7348", decoded[1].Address);
            Assert.Equal((ushort)5678, decoded[1].Port);
            
            Assert.Equal("5.0.7.255", decoded[2].Address);
            Assert.Equal(ushort.MaxValue, decoded[2].Port);
            
            Assert.Equal("fe80::1ff:fe23:4567:890a", decoded[3].Address);
            Assert.Equal((ushort)0, decoded[3].Port);
        }
    }
}