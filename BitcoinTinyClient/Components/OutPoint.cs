using System;
using System.Linq;

namespace BitcoinTinyClient.Components
{
    public class OutPoint
    {
        public byte[] Hash { get; }
        private uint Index { get; }

        public OutPoint(byte[] hash, uint index)
        {
            Hash = new byte[hash.Length];

            hash.CopyTo(Hash, 0);
            Hash = Hash.Reverse().ToArray();

            Index = index;
        }

        public byte[] ToBytes()
        {
            return Hash
                    .Concat(BitConverter.GetBytes(Index))
                    .ToArray();
        }
    }
}
