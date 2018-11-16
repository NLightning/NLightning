using System;
using NLightning.Transport;
using NLightning.Utils;
using NLightning.Utils.Extensions;
using Xunit;

namespace NLightning.Test.Transport
{
    public class NodeAddressTests
    {
        [Fact]
        public void ParseTest()
        {
            NodeAddress address = NodeAddress.Parse("0242a4ae0c5bef18048fbecf995094b74bfb0f7391418d71ed394784373f41e4f3@18.184.237.59:9735");
            
            Assert.Equal("18.184.237.59", address.IpAddress);
            Assert.Equal(9735, address.Port);
            Assert.Equal("0242a4ae0c5bef18048fbecf995094b74bfb0f7391418d71ed394784373f41e4f3", 
                address.PublicKey.PublicKeyCompressed.ToHex());
        }
        
        [Fact]
        public void ParseDefaultPortTest()
        {
            NodeAddress address = NodeAddress.Parse("0242a4ae0c5bef18048fbecf995094b74bfb0f7391418d71ed394784373f41e4f3@18.184.237.59");
            
            Assert.Equal("18.184.237.59", address.IpAddress);
            Assert.Equal(9735, address.Port);
            Assert.Equal("0242a4ae0c5bef18048fbecf995094b74bfb0f7391418d71ed394784373f41e4f3", 
                address.PublicKey.PublicKeyCompressed.ToHex());
        }
        
        [Fact]
        public void ParseInvalidPubKeyTest()
        {
            Assert.Throws<ArgumentException>(() => NodeAddress.Parse("0242a4ae0c5bef18048fbecf995094b74bfb0f739@18.184.237.59:9735"));
        }
        
        [Fact]
        public void ParseInvalidAddressTest1()
        {
            Assert.Throws<ArgumentException>(() => NodeAddress.Parse("0242a4ae0c5bef18048fbecf995094b74bfb0f7391418d71ed394784373f41e4f3@18@184.237.59:9735"));
        }
       
    }
}