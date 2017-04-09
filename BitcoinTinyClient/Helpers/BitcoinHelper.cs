using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;

namespace BitcoinTinyClient.Helpers
{
    public static class BitcoinHelper
    {
        public static int GetCurrentTimeStamp()
        {
            return (int)DateTime.UtcNow
                        .Subtract(new DateTime(1970, 1, 1))
                        .TotalSeconds;
        }

        public static byte[] CreatePublicKeyFromPrivate(byte[] privateKey)
        {
            privateKey = (new byte[] { 0x00 }).Concat(privateKey).ToArray();

            var secp256K1Algorithm = SecNamedCurves.GetByName("secp256k1");
            var privateKeyInteger = new BigInteger(privateKey);

            var multiplication = secp256K1Algorithm.G.Multiply(privateKeyInteger).Normalize();
            var publicKey = new byte[65];

            var y = multiplication.YCoord.ToBigInteger().ToByteArray();
            Array.Copy(y, 0, publicKey, 64 - y.Length + 1, y.Length);

            var x = multiplication.XCoord.ToBigInteger().ToByteArray();
            Array.Copy(x, 0, publicKey, 32 - x.Length + 1, x.Length);

            publicKey[0] = 0x04;

            return publicKey;
        }

        public static byte[] Create160BitHashFromPublicKey(byte[] publicKey)
        {
            var sha256Algorithm = new SHA256Managed();
            var ripemd160Algorithm = new RIPEMD160Managed();

            var sha256Hash = sha256Algorithm.ComputeHash(publicKey);
            var ripemd160Hash = ripemd160Algorithm.ComputeHash(sha256Hash);

            return ripemd160Hash;
        }

        public static string CreateBitcoinAddressFrom160BitHash(byte[] publicKeyHash)
        {
            byte[] firstHashByte = { 0x00 };

            var extendedHash = firstHashByte.Concat(publicKeyHash).ToArray();

            var addressChecksum = CalculateHash(extendedHash)
                                  .Take(4).ToArray();

            var extendedAddress = extendedHash.Concat(addressChecksum).ToArray();

            return Base58CheckEncode(extendedAddress);
        }

        public static string CreateBitcoinAddressFrom160BitPublicKeyHash(byte[] publicKeyHash)
        {
            byte[] firstHashByte = { 0x00 };

            var extendedHash = firstHashByte.Concat(publicKeyHash).ToArray();
            var addressChecksum = Enumerable.Take<byte>(CalculateHash(extendedHash), 4).ToArray();
            var extendedAddress = extendedHash.Concat(addressChecksum).ToArray();

            return Base58CheckEncode(extendedAddress);
        }

        public static string Create160BitPublicKeyFromBitcoinAddress(string bitcoinAddress)
        {
            var base58CheckDecodedAddress = Base58CheckDecode(bitcoinAddress);

            return StringHelper.ByteArrayToString(base58CheckDecodedAddress.Skip(1).Take(20).ToArray());
        }

        public static string Base58CheckEncode(byte[] bytes)
        {
            const string base58CheckCodeString =
              "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

            var result = string.Empty;
            var leadingZeros = 0;

            do
            {
                if (bytes[leadingZeros] == 0x00)
                {
                    ++leadingZeros;
                }


            } while (bytes[leadingZeros] == 0x00 && leadingZeros < bytes.Length);

            bytes = bytes.Reverse()
                      .Take(bytes.Length - leadingZeros)
                      .Concat(new byte[] { 0x00 })
                      .ToArray();

            var bigInteger = new System.Numerics.BigInteger(bytes);

            while (bigInteger > 0)
            {
                var remainder = bigInteger % 58;

                result += base58CheckCodeString[(int)remainder];
                bigInteger = bigInteger / 58;
            }

            for (var i = 0; i < leadingZeros; ++i)
            {
                result += base58CheckCodeString[0];
            }

            return new string(result.ToCharArray().Reverse().ToArray());
        }

        public static byte[] Base58CheckDecode(string base58CheckTextToDecode)
        {
            const string base58CheckCodeString = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

            var bigInteger = new System.Numerics.BigInteger(0);

            var leadingZeros = 0;

            do
            {
                if (base58CheckTextToDecode[leadingZeros] == base58CheckCodeString[0])
                    ++leadingZeros;

            } while (base58CheckTextToDecode[leadingZeros] == base58CheckCodeString[0] && leadingZeros < base58CheckTextToDecode.Length);

            base58CheckTextToDecode = new string(base58CheckTextToDecode.ToCharArray().Skip(leadingZeros).ToArray());

            for (var i = 0; i < base58CheckTextToDecode.Length; ++i)
            {
                bigInteger *= 58;
                bigInteger += base58CheckCodeString.IndexOf(base58CheckTextToDecode[i]);
            }

            var bigIntegerArray = bigInteger.ToByteArray().Reverse().ToArray();
            bigIntegerArray = bigIntegerArray.SkipWhile(b => b == 0x00).ToArray();

            var leadingZerosArray = new byte[leadingZeros];

            for (var i = 0; i < leadingZeros; i++)
            {
                leadingZerosArray[i] = 0x00;
            }

            return leadingZerosArray.Concat(bigIntegerArray).ToArray();
        }

        public static string ConvertPrivateKeyToWif(byte[] privateKey)
        {
            byte[] firstByte = { 0x80 };

            var extendedPrivateKey = firstByte.Concat(privateKey).ToArray();
            var checksum = CalculateHash(extendedPrivateKey);

            extendedPrivateKey = extendedPrivateKey
                                   .Concat(checksum.Take(4).ToArray())
                                   .ToArray();

            return Base58CheckEncode(extendedPrivateKey);
        }

        public static byte[] Sign(byte[] bytes, byte[] privateKey)
        {
            var x9EcParameters = SecNamedCurves.GetByName("secp256k1");
            var ecParams = new ECDomainParameters(x9EcParameters.Curve, x9EcParameters.G, x9EcParameters.N, x9EcParameters.H);

            var privateKeyBigInteger = new BigInteger((new byte[] { 0x00 }).Concat(privateKey).ToArray());

            var signer = new ECDsaSigner();

            var privateKeyParameters = new ECPrivateKeyParameters(privateKeyBigInteger, ecParams);
            signer.Init(true, privateKeyParameters);

            var signature = signer.GenerateSignature(bytes);

            var memoryStream = new MemoryStream();

            var sequenceGenerator = new DerSequenceGenerator(memoryStream);
            sequenceGenerator.AddObject(new DerInteger(signature[0]));
            sequenceGenerator.AddObject(new DerInteger(signature[1]));
            sequenceGenerator.Close();

            var signingResult = memoryStream.ToArray();

            return signingResult;
        }

        public static uint CalculateChecksum(byte[] bytes)
        {
            var hash = CalculateHash(bytes);

            return BitConverter.ToUInt32(hash, 0);
        }

        public static byte[] CalculateHash(byte[] bytes)
        {
            var sha256Managed = new SHA256Managed();

            return sha256Managed.ComputeHash(sha256Managed.ComputeHash(bytes));
        }
    }
}