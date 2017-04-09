using System.Linq;
using BitcoinTinyClient.Helpers;

namespace BitcoinTinyClient.Components
{
    public class ScriptPubKey
    {
        const byte OP_DUP = 0x76;
        const byte OP_HASH160 = 0xa9;
        const byte PUSHDATA14 = 0x14;
        const byte OP_EQUALVERIFY = 0x88;
        const byte OP_CHECKSIG = 0xac;

        private readonly byte[] _scriptPubKey;

        public int Length => GetBytesCount();

        public ScriptPubKey(string publicKey160HashHex)
        {
            _scriptPubKey =
                new[] { OP_DUP, OP_HASH160, PUSHDATA14 }
                    .Concat(StringHelper.HexStringToByteArray(publicKey160HashHex))
                    .Concat(new[] { OP_EQUALVERIFY, OP_CHECKSIG })
                    .ToArray();
        }

        public byte[] ToBytes()
        {
            return _scriptPubKey;
        }

        private int GetBytesCount()
        {
            return ToBytes().Length;
        }
    }
}
