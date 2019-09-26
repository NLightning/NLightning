using System;
using System.Linq;
using NLightning.Cryptography;
using NLightning.Utils.Extensions;

namespace NLightning.Transport
{
    public class NodeAddress
    {
        public ECKeyPair PublicKey { get; private set; }
        public String IpAddress { get; private set; }
        public int Port { get; private set; }

        public NodeAddress(ECKeyPair publicKey, String ipAddress, int port)
        {
            PublicKey = publicKey;
            IpAddress = ipAddress;
            Port = port;
        }

        public string Address => ToString();

        public override string ToString()
        {
            string address = PublicKey.PublicKeyCompressed.ToHex() + "@" + IpAddress;
            
            if (Port != 0)
            {
                address = address + ":" + Port;
            }

            return address;
        }

        public override bool Equals(object obj)
        {
            return Address == ((NodeAddress)obj).Address;
        }

        public override int GetHashCode()
        {
            return base.ToString().GetHashCode();
        }

        public static NodeAddress Parse(String address)
        {
            var pubKeyAddressSplit = address.Split('@');
            if (pubKeyAddressSplit.Length != 2)
            {
                throw new ArgumentException("Invalid address format.", "address");
            }
            
            String publicKey = pubKeyAddressSplit.First();
            String ipAndPort = pubKeyAddressSplit.Last();
            var ipPortSplit = ipAndPort.Split(':');
            
            String ipAddress = ipPortSplit.First();
            int port = ipAndPort.Length == 2 ? Int32.Parse(ipPortSplit.Last()) : 9735;

            if (publicKey.Length != 66)
            {
                throw new ArgumentException("Invalid Public Key length", "address");
            }
            
            return new NodeAddress(new ECKeyPair(publicKey), ipAddress, port);
        }
    }
}