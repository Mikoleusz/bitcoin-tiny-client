using System;
using System.Linq;

namespace BitcoinTinyClient.Components
{
    public class TxOut
    {
        public ulong Value { get; }
        public VarInt ScriptPubKeyLength { get; }
        public ScriptPubKey ScriptPubKey { get; }

        public TxOut(ulong value, ScriptPubKey scriptPubKey)
        {
            Value = value;
            ScriptPubKeyLength = new VarInt(Convert.ToUInt64(scriptPubKey.Length));
            ScriptPubKey = scriptPubKey;
        }

        public byte[] ToBytes()
        {
            var result = BitConverter.GetBytes(Value)
                .Concat(ScriptPubKeyLength.ToBytes())
                .Concat(ScriptPubKey.ToBytes())
                .ToArray();

            return result;
        }
    }
}
