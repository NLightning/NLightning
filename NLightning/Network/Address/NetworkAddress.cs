using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLightning.Utils.Extensions;

namespace NLightning.Network.Address
{
    public class NetworkAddress
    {
        public NetworkAddress(AddressType type, String address, ushort port)
        {
            Type = type;
            Address = address;
            Port = port;
        }
        
        public AddressType Type { get; }
        public String Address { get; }
        public ushort Port { get; }
        
        public static byte[] Encode(List<NetworkAddress> addresses)
        {
            List<byte[]> addressData = new List<byte[]>();
            foreach (var address in addresses)
            {
                switch (address.Type)
                {
                    case AddressType.IpV4:
                        addressData.Add(EncodeIpV4(address));
                        break;
                    case AddressType.IpV6:
                        addressData.Add(EncodeIpV6(address));
                        break;
// TODO: support TOR
//                    case AddressType.TorV2:
//                        byte[] torv2 = new byte[12];
//                        torv2[0] = 2;
//                        break;
//                    case AddressType.TorV3:
//                        byte[] torv3 = new byte[37];
//                        torv3[0] = 3;
//                        addressData.Add(torv3);
//                        break;
                }
            }

            return ByteExtensions.Combine(addressData.ToArray());
        }

        private static byte[] EncodeIpV6(NetworkAddress address)
        {
            byte[] port = address.Port.GetBytesBigEndian();
            var ip2 = IPAddress.Parse(address.Address);
            var byteData2 = ip2.GetAddressBytes().ToArray();
            var encoded = new byte[19];
            
            encoded[0] = 2;
            Array.Copy(port, 0, encoded, 17, 2);
            Array.Copy(byteData2, 0, encoded, 1, 16);
            return encoded;
        }

        private static byte[] EncodeIpV4(NetworkAddress address)
        {
            byte[] port = address.Port.GetBytesBigEndian();
            var encoded = new byte[7];
            var ip = IPAddress.Parse(address.Address);
            var byteData = ip.GetAddressBytes().ToArray();

            encoded[0] = 1;
            Array.Copy(byteData, 0, encoded, 1, 4);
            Array.Copy(port, 0, encoded, 5, 2);
            return encoded;
        }

        public static List<NetworkAddress> Decode(Span<byte> data)
        {
            List<NetworkAddress> networkAddresses = new List<NetworkAddress>();
            int position = 0;
            while (position < data.Length)
            {
                int length;
                switch (data[position])
                {
                    case 1:
                        length = 7;
                        networkAddresses.Add(DecodeIpV4(data.Slice(position + 1, 6).ToArray()));
                        break;
                    case 2:
                        length = 19;
                        networkAddresses.Add(DecodeIpV6(data.Slice(position + 1, 18).ToArray()));
                        break;
                    case 3:
                        length = 13;
                        break;
                    case 4:
                        length = 38;
                        break;
                    default:
                        length = data.Length;
                        break;
                }

                position += length;
            }

            return networkAddresses;
        }

        private static NetworkAddress DecodeIpV6(byte[] data)
        {
            ushort port = data.SubArray(data.Length - 2, 2).ToUShortBigEndian();
            IPAddress ipAddress = new IPAddress(data.SubArray(0, data.Length - 2));
            return new NetworkAddress(AddressType.IpV6, ipAddress.ToString(), (ushort)port);
        }

        private static NetworkAddress DecodeIpV4(byte[] data)
        {
            ushort port = data.SubArray(data.Length - 2, 2).ToUShortBigEndian();
            IPAddress ipAddress = new IPAddress(data.SubArray(0, data.Length - 2));
            return new NetworkAddress(AddressType.IpV4, ipAddress.ToString(), (ushort)port);
        }
    }
}