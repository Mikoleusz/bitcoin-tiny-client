using System;
using System.Text;

namespace BitcoinTinyClient.Helpers
{
    public class StringHelper
    {
        public static byte[] HexStringToByteArray(string text)
        {
            var bytes = new byte[text.Length / 2];

            for (var i = 0; i < text.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte($"{text[i]}{text[i + 1]}", 16);
            }

            return bytes;
        }

        public static string ByteArrayToString(byte[] bytes, int length)
        {
            var hex = new StringBuilder();

            for (var i = 0; i < length; ++i)
            {
                hex.AppendFormat("{0:x2}", bytes[i]);
            }

            return hex.ToString().ToUpper();
        }

        public static string ByteArrayToString(byte[] bytes)
        {
            return ByteArrayToString(bytes, bytes.Length);
        }
    }
}
