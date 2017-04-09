using System;
using System.Linq;

namespace BitcoinTinyClient.Components
{
    public class ShortNetworkAddress
    {
        public ulong Services { get; }
        public byte[] Ip { get; }
        public ushort Port { get; }

        public string IpString => GetIpString();

        public ShortNetworkAddress(byte[] ip, ushort port)
        {
            Services = 1;
            Ip = ip;
            Port = port;
        }

        public byte[] ToBytes()
        {
            return BitConverter.GetBytes(Services)
              .Concat(GetIpBytes())
              .Concat(GetPortBytes())
              .ToArray();
        }

        private byte[] GetIpBytes()
        {
            var result = new byte[16];

            for (var i = 0; i < 10; ++i)
            {
                result[i] = 0;
            }

            result[10] = result[11] = 0xFF;

            for (var i = 0; i < 4; i++)
            {
                result[12 + i] = Ip[i];
            }

            return result;
        }

        private byte[] GetPortBytes()
        {
            return BitConverter.GetBytes(Port).Reverse().ToArray();
        }

        private string GetIpString()
        {
            return $"{Ip[0]}.{Ip[1]}.{Ip[2]}.{Ip[3]}";
        }
    }

}
