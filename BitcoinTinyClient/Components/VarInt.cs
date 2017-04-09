using System;
using System.Linq;

namespace BitcoinTinyClient.Components
{
    public class VarInt
    {
        public ulong Value { get; }

        public VarInt(ulong value)
        {
            Value = value;
        }

        public byte[] ToBytes()
        {
            var bytes = new byte[1];

            if (Value < 0xFD)
            {
                bytes[0] = Convert.ToByte(Value);
                return bytes;
            }

            if (Value <= 0xFFFF)
            {
                bytes[0] = 0xFD;
                return bytes.Concat(
                         BitConverter.GetBytes(Convert.ToUInt16(Value))).ToArray();
            }

            if (Value <= 0xFFFFFFFF)
            {
                bytes[0] = 0xFE;
                return bytes.Concat(
                         BitConverter.GetBytes(Convert.ToUInt32(Value))).ToArray();
            }

            bytes[0] = 0xFF;
            return bytes.Concat(BitConverter.GetBytes(Value)).ToArray();
        }
    }
}
