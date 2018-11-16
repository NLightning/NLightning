using System;
using System.Collections.Generic;
using System.Linq;
using DnsClient;
using DnsClient.Protocol;
using Microsoft.Extensions.Logging;
using NBitcoin.DataEncoders;
using NLightning.Cryptography;
using NLightning.Network;
using NLightning.Transport;

namespace NLightning.Peer.Discovery
{
    public class DnsBootstrap : IPeerDiscovery
    {
        private readonly ILogger _logger;
        private readonly bool _ipv6;
        private readonly NetworkParameters _networkParameters;

        public DnsBootstrap(ILogger logger, NetworkParameters networkParameters, bool ipv6 = false)
        {
            _logger = logger;
            _ipv6 = ipv6;
            _networkParameters = networkParameters;
        }
        
        public List<NodeAddress> FindNodes(int nodeCount)
        {
            var client = new LookupClient();
            var list = new List<NodeAddress>();
            var seeds = _networkParameters.DnsSeeds.OrderBy(a => Guid.NewGuid())
                            .Select((seed) => GetQueryUrl(seed, _networkParameters.DnsNetworkRealm))
                            .ToList();
            
            foreach (var dnsSeed in seeds)
            {
                if (list.Count < nodeCount)
                {
                    try
                    {
                        var srvResult = client.Query(dnsSeed, QueryType.SRV);
                        var srvShuffled = srvResult.Answers.OrderBy(srv => Guid.NewGuid()).ToList();

                        foreach (var srv in srvShuffled.SrvRecords())
                        {
                            if (list.Count < nodeCount)
                            {
                                var result = client.Query(srv.Target, _ipv6 ? QueryType.AAAA : QueryType.A);
                                
                                if (result.Answers.Count > 0)
                                {
                                    ECKeyPair publicKey = GetPublicKey(srv);
                                    string ip = GetIp(result.Answers[0]);

                                    if (ip != "0.0.0.0" && ip != "[::0]")
                                    {
                                        list.Add(new NodeAddress(publicKey, ip, srv.Port));
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError("failed to get dns seed nodes: " + exception);
                    }
                }
            }

            return list;
        }

        private string GetQueryUrl(string seed, int networkRealm)
        {
            string ipTypeQuery = _ipv6 ? "a4" : "a2";
            string realm = "r" + networkRealm;
            return $"{realm}.{ipTypeQuery}.{seed}";
        }

        private string GetIp(DnsResourceRecord answer)
        {
            if (answer is ARecord)
            {
                return ((ARecord) answer).Address.ToString();
            }

            return $"[{((AaaaRecord) answer).Address}]";
        }

        private ECKeyPair GetPublicKey(SrvRecord srv)
        {
            string bech32 = srv.Target.Value.Split(".").First();

            Bech32Encoder bech32Encoder = Encoders.Bech32("ln");
            var bech32Data5Bits = bech32Encoder.DecodeData(bech32);
            var bech32Data8Bits = ConvertBits(bech32Data5Bits, 5, 8, false);
            
            return new ECKeyPair(bech32Data8Bits, false);
        }
        
        /*
         * The following method was copied from NBitcoin
         * https://github.com/MetacoSA/NBitcoin/blob/23beaaab48f2038dca24a6020e71cee0b14cd55f/NBitcoin/DataEncoders/Bech32Encoder.cs#L427
         * TODO: ask NBitcoin to expose ConvertBits method
         */
        private static byte[] ConvertBits(IEnumerable<byte> data, int fromBits, int toBits, bool pad = true)
        {
            int num1 = 0;
            int num2 = 0;
            int num3 = (1 << toBits) - 1;
            List<byte> byteList = new List<byte>();
            foreach (byte num4 in data)
            {
                if ((int) num4 >> fromBits > 0)
                    throw new FormatException("Invalid Bech32 string");
                num1 = num1 << fromBits | (int) num4;
                num2 += fromBits;
                while (num2 >= toBits)
                {
                    num2 -= toBits;
                    byteList.Add((byte) (num1 >> num2 & num3));
                }
            }
            if (pad)
            {
                if (num2 > 0)
                    byteList.Add((byte) (num1 << toBits - num2 & num3));
            }
            else if (num2 >= fromBits || (byte) (num1 << toBits - num2 & num3) != (byte) 0)
                throw new FormatException("Invalid Bech32 string");
            
            return byteList.ToArray();
        }
    }
}