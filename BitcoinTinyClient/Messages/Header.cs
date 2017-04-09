using System;
using System.Linq;
using System.Text;
using BitcoinTinyClient.Helpers;

namespace BitcoinTinyClient.Messages
{
    public class Header
    {
        public uint Magic { get; }
        public string Command { get; }
        public uint PayloadLength { get; set; }
        public uint Checksum { get; set; }

        public Header(string command)
        {
            var payload = new byte[] { };

            Magic = 0xD9B4BEF9;
            Command = command.PadRight(12, '\0');
            PayloadLength = 0;
            Checksum = BitcoinHelper.CalculateChecksum(payload);
        }

        public Header(byte[] bytes)
        {
            Magic = BitConverter.ToUInt32(bytes, 0);
            Command = Encoding.ASCII.GetString(bytes, 4, 12);
            PayloadLength = BitConverter.ToUInt32(bytes, 16);
            Checksum = BitConverter.ToUInt32(bytes, 20);
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Magic)
                .Concat(Encoding.ASCII.GetBytes(Command))
                .Concat(BitConverter.GetBytes(PayloadLength))
                .Concat(BitConverter.GetBytes(Checksum))
                .ToArray();
        }

        public void CalculateChecksum(byte[] payload)
        {
            PayloadLength = Convert.ToUInt32(payload.Length);
            Checksum = BitcoinHelper.CalculateChecksum(payload);
        }
    }
}