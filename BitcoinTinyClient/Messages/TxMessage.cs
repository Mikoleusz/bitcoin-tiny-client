using System;
using System.Collections.Generic;
using System.Linq;
using BitcoinTinyClient.Components;
using BitcoinTinyClient.Helpers;

namespace BitcoinTinyClient.Messages
{
    public class TxMessage
    {
        public Header Header { get; }
        public uint Version { get; }
        public VarInt TxInsCount { get; set; }
        public List<TxIn> TxIns { get; set; }
        public VarInt TxOutsCount { get; set; }
        public List<TxOut> TxOuts { get; set; }
        public uint LockTime { get; }

        public TxMessage()
        {
            Header = new Header("tx");
            Version = 1;
            TxInsCount = new VarInt(0);
            TxIns = new List<TxIn>();
            TxOutsCount = new VarInt(0);
            TxOuts = new List<TxOut>();
            LockTime = 0x00;
        }

        public void ComputeHeader()
        {
            var payload = GetPayload();

            Header.PayloadLength = Convert.ToUInt32(payload.Length);
            Header.CalculateChecksum(payload);
        }

        private byte[] GetPayload()
        {
            var payload = BitConverter.GetBytes(Version)
                .Concat(TxInsCount.ToBytes()).ToArray();

            foreach (var txIn in TxIns)
            {
                payload = payload.Concat(txIn.ToBytes()).ToArray();
            }

            payload = payload.Concat(TxOutsCount.ToBytes()).ToArray();

            foreach (var txOut in TxOuts)
            {
                payload = payload.Concat(txOut.ToBytes()).ToArray();
            }

            payload = payload.Concat(BitConverter.GetBytes(LockTime)).ToArray();

            return payload;
        }

        public void Sign(byte[] privateKey)
        {
            var publicKey = BitcoinHelper.CreatePublicKeyFromPrivate(privateKey);

            var transactionHash = ComputeHashBeforeSigning();

            var signature = BitcoinHelper.Sign(transactionHash, privateKey);

            TxIns.First().CreateSignatureScript(signature, publicKey);
        }

        public byte[] ComputeHashBeforeSigning()
        {
            var SIGHASH_ALL = new byte[] { 0x01, 0x00, 0x00, 0x00 };

            var payload = GetPayload();
            var extendedPayload = payload.Concat(SIGHASH_ALL).ToArray();

            var hash = BitcoinHelper.CalculateHash(extendedPayload);

            return hash;
        }

        public byte[] ToBytes()
        {
            ComputeHeader();

            var header = Header.ToBytes();
            var payload = GetPayload();

            return header.Concat(payload).ToArray();
        }

        public static TxMessage Create(
            string previousTransactionIdHex,
            string senderBitcoinAddress,
            string senderPrivateKeyHex,
            string recipientBitcoinAddress,
            double transactionAmountInBtc)
        {
            var txMessage = new TxMessage();

            var senderPrivateKey = StringHelper.HexStringToByteArray(senderPrivateKeyHex);

            var scriptPubKeyPreviousTransaction = new ScriptPubKey(
                BitcoinHelper.Create160BitPublicKeyFromBitcoinAddress(senderBitcoinAddress)
            );

            var scriptPubKeyCurrentTransaction = new ScriptPubKey(
                BitcoinHelper.Create160BitPublicKeyFromBitcoinAddress(recipientBitcoinAddress)
            );

            var txInOutPoint = new OutPoint(
                StringHelper.HexStringToByteArray(previousTransactionIdHex), 0
            );

            txMessage.TxInsCount = new VarInt(1);
            txMessage.TxIns = new List<TxIn>
            {
                new TxIn(txInOutPoint, scriptPubKeyPreviousTransaction.ToBytes())
            };

            txMessage.TxOutsCount = new VarInt(1);
            txMessage.TxOuts = new List<TxOut>
            {
                new TxOut(
                    Convert.ToUInt64(transactionAmountInBtc*100000000),
                    scriptPubKeyCurrentTransaction
                )
            };

            txMessage.Sign(senderPrivateKey);

            return txMessage;
        }
    }
}