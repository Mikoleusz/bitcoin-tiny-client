using System;
using System.Linq;

namespace BitcoinTinyClient.Components
{
    public class TxIn
    {
        public OutPoint PreviousOutput { get; }
        public VarInt ScriptSigLength { get; set; }
        public byte[] ScriptSig { get; set; }
        public uint Sequence { get; }

        public TxIn(OutPoint previousOutput, byte[] scriptSig)
        {
            PreviousOutput = previousOutput;
            ScriptSigLength = new VarInt(Convert.ToUInt64(scriptSig.Length));
            ScriptSig = scriptSig;

            Sequence = 0xFFFFFFFF;
        }

        public void CreateSignatureScript(byte[] signature, byte[] publicKey)
        {
            const byte SIGHASH_ALL = 0x01;

            var extendedSignature = signature.Concat(new[] { SIGHASH_ALL }).ToArray();

            var extendedSignatureLength = new[] { Convert.ToByte(extendedSignature.Length) };
            var publicKeyLength = new[] { Convert.ToByte(publicKey.Length) };

            ScriptSig =
                extendedSignatureLength.Concat(extendedSignature)
                    .Concat(publicKeyLength)
                    .Concat(publicKey)
                    .ToArray();

            ScriptSigLength = new VarInt(Convert.ToUInt64(ScriptSig.Length));
        }

        public byte[] ToBytes()
        {
            return PreviousOutput.ToBytes()
                    .Concat(ScriptSigLength.ToBytes())
                    .Concat(ScriptSig)
                    .Concat(BitConverter.GetBytes(Sequence))
                    .ToArray();
        }
    }
}
