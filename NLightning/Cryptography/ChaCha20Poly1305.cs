using System;
using NLightning.Utils.Extensions;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace NLightning.Cryptography
{
    public static class ChaCha20Poly1305
    {
        public static (byte[], byte[]) EncryptWithAdditionalData(byte[] key, byte[] nonce, byte[] additionalData, 
                                                                 byte[] plaintext, bool skipFirst64Byte = true)
        {            
            var polyKey = ChaCha20Encrypt(new byte[32], key, nonce);
            var cipherText = ChaCha20Encrypt(plaintext, key, nonce, skipFirst64Byte);
            var data = ByteExtensions.Combine(additionalData, Pad16(additionalData), 
                cipherText,Pad16(cipherText),
                BitConverter.GetBytes((long) additionalData.Length),
                BitConverter.GetBytes((long) cipherText.Length));

            var macTag = CalculateMac(polyKey, data);
            
            return (cipherText, macTag);
        }
        
        private static byte[] Pad16(byte[] data)
        {
            if (data.Length % 16 == 0)
            {
                return new byte[0];
            }

            return new byte[16 - (data.Length % 16)];
        }

        private static byte[] CalculateMac(byte[] key, byte[] data)
        {
            var tag = new byte[16];
            Poly1305 poly = new Poly1305();
            poly.Init(new KeyParameter(key));
            poly.BlockUpdate(data, 0, data.Length);
            poly.DoFinal(tag, 0);
           
            return tag;
        }
        
        public static (byte[] plainText, byte[] mac) DecryptWithAdditionalData(byte[] key, byte[] nonce, byte[] additionalData, byte[] cipherText)
        {
            var polyKey = ChaCha20Encrypt(new byte[32], key, nonce);
            var data = ByteExtensions.Combine(additionalData, Pad16(additionalData),
                cipherText, Pad16(cipherText),
                BitConverter.GetBytes((long) additionalData.Length),
                BitConverter.GetBytes((long) cipherText.Length));

            var mac = CalculateMac(polyKey, data);
            var plainText = ChaCha20Decrypt(cipherText, key, nonce, true);
            return (plainText, mac);
        }

        private static byte[] ChaCha20Decrypt(byte[] cipherText, byte[] key, byte[] nonce, bool skipFirst64Byte = false)
        {
            var engine = new ChaCha7539Engine();
            
            engine.Init(true, new ParametersWithIV(new KeyParameter(key), nonce));
            var plainText = new byte[cipherText.Length];

            if (skipFirst64Byte)
            {
                engine.ProcessBytes(new byte[64], 0, 64, new byte[64], 0);
            }
            
            engine.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
            return plainText;
        }
        
        public static byte[] ChaCha20Encrypt(byte[] plainText, byte[] key, byte[] nonce, bool skipFirst64Byte = false)
        {
            var engine = new ChaCha7539Engine();
            
            engine.Init(true, new ParametersWithIV(new KeyParameter(key), nonce));
            var cipherText = new byte[plainText.Length];

            if (skipFirst64Byte)
            {
                engine.ProcessBytes(new byte[64], 0, 64, new byte[64], 0);
            }
            
            engine.ProcessBytes(plainText, 0, plainText.Length, cipherText, 0);
            return cipherText;
        }
    }
}