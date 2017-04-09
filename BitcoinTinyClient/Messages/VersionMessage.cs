using System;
using System.Linq;
using System.Text;
using BitcoinTinyClient.Components;
using BitcoinTinyClient.Helpers;

namespace BitcoinTinyClient.Messages
{
    public class VersionMessage
    {
        public Header Header { get; }
        public int ProtocolVersion { get; }
        public ulong Services { get; }
        public long Timestamp { get; }
        public ShortNetworkAddress RecipientAddress { get; }
        public ShortNetworkAddress SenderAddress { get; }
        public ulong Nonce { get; }
        public VarInt UserAgentLength { get; }
        public string UserAgent { get; }
        public int StartHeight { get; }
        public bool Relay { get; }

        public VersionMessage(ShortNetworkAddress recipientAddress,
                       ShortNetworkAddress senderAddress)
        {
            Header = new Header("version");
            ProtocolVersion = 70002;
            Services = 1;
            Timestamp = BitcoinHelper.GetCurrentTimeStamp();
            RecipientAddress = recipientAddress;
            SenderAddress = senderAddress;
            Nonce = 0;
            UserAgent = "/MikoleuszBlogBitcoinTinyClient:1.0/";
            UserAgentLength = new VarInt(Convert.ToUInt64(UserAgent.Length));
            StartHeight = 460650;
            Relay = true;

            ComputeHeader();
        }

        public byte[] ToBytes()
        {
            var header = Header.ToBytes();
            var payload = GetPayload();

            return header.Concat(payload).ToArray();
        }

        private void ComputeHeader()
        {
            var payload = GetPayload();

            Header.CalculateChecksum(payload);
        }

        private byte[] GetPayload()
        {
            return BitConverter.GetBytes(ProtocolVersion)
                    .Concat(BitConverter.GetBytes(Services))
                    .Concat(BitConverter.GetBytes(Timestamp))
                    .Concat(RecipientAddress.ToBytes())
                    .Concat(SenderAddress.ToBytes())
                    .Concat(BitConverter.GetBytes(Nonce))
                    .Concat(UserAgentLength.ToBytes())
                    .Concat(Encoding.ASCII.GetBytes(UserAgent))
                    .Concat(BitConverter.GetBytes(StartHeight))
                    .Concat(Relay ? new byte[] { 0x01 } : new byte[] { 0x00 })
                    .ToArray();
        }
    }
}
