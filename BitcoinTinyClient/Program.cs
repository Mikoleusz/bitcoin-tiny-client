using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using BitcoinTinyClient.Components;
using BitcoinTinyClient.Messages;

namespace BitcoinTinyClient
{
    class Program
    {
        static void Main()
        {
            var senderIpAddress = new byte[] { 0, 0, 0, 0 };
            var recipientIpAddress = new byte[] { 0, 0, 0, 0 };
            var previousTransactionIdHex = "<previousTransactionIdHex>";
            var senderBitcoinAddress = "<senderBitcoinAddress>";
            var senderPrivateKeyHex = "<senderPrivateKeyHex>";
            var recipientBitcoinAddress = "<recipientBitcoinAddress>";
            var transactionAmountInBtc = 0;

            PerformTransaction(
                senderIpAddress,
                recipientIpAddress,
                previousTransactionIdHex,
                senderBitcoinAddress,
                senderPrivateKeyHex,
                recipientBitcoinAddress,
                transactionAmountInBtc);
        }

        public static void PerformTransaction(
            byte[] senderIpAddress,
            byte[] recipientIpAddress,
            string previousTransactionIdHex,
            string senderBitcoinAddress,
            string senderPrivateKeyHex,
            string recipientBitcoinAddress,
            double transactionAmountInBtc)
        {
            var senderShortNetworkAddress =
                new ShortNetworkAddress(senderIpAddress, 8333);

            var recipientShortNetworkAddress =
                new ShortNetworkAddress(recipientIpAddress, 8333);

            var versionMessage =
                new VersionMessage(recipientShortNetworkAddress, senderShortNetworkAddress);

            var transactionMessage = TxMessage.Create(
                    previousTransactionIdHex,
                    senderBitcoinAddress,
                    senderPrivateKeyHex,
                    recipientBitcoinAddress,
                    transactionAmountInBtc);

            var tcpClient = new TcpClient(recipientShortNetworkAddress.IpString, recipientShortNetworkAddress.Port);
            var tcpClientStream = tcpClient.GetStream();

            var versionMessageBytes = versionMessage.ToBytes();
            var transactionMessageBytes = transactionMessage.ToBytes();

            tcpClientStream.Write(versionMessageBytes, 0, versionMessageBytes.Length);
            tcpClientStream.Write(transactionMessageBytes, 0, transactionMessageBytes.Length);

            var buffer = new byte[0];

            while (true)
            {
                var bytesToReadCount = tcpClient.Available;

                if (bytesToReadCount == 0)
                {
                    Thread.Sleep(100);
                    continue;
                }

                var readedData = new byte[bytesToReadCount];
                tcpClientStream.Read(readedData, 0, bytesToReadCount);

                buffer = buffer.Concat(readedData).ToArray();

                var header = new Header(buffer);

                Console.WriteLine(
                  $"{DateTime.Now.ToLongTimeString()}: " +
                  $"incoming message: {header.Command}. " +
                  $"Length: {header.PayloadLength}");

                buffer = buffer.Skip(24 + (int)header.PayloadLength).ToArray();
            }
        }
    }
}
