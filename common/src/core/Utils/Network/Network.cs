using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// Utils for network programming.
/// </summary>
namespace VVVV.Utils.Network
{
    public static class Network
    {
        //print host info
        public static void PrintHostInfo(String host)
        {
            try
            {

                //try to resolve host name into IP
                IPHostEntry hostInfo = Dns.GetHostEntry(host);

                //name
                Console.WriteLine("Canonical Name: " + hostInfo.HostName);

                //all IP adresses
                Console.Write("IP Addresses: ");

                foreach (IPAddress ipaddr in hostInfo.AddressList)
                {
                    Console.Write(ipaddr.ToString() + " ");
                }

                //does this host have aliases?
                if (hostInfo.Aliases.Length > 0)
                {
                    Console.WriteLine("Aliases: ");

                    foreach (String alias in hostInfo.Aliases)
                    {
                        Console.Write(alias + " ");
                    }
                }

                Console.WriteLine();
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to resolve host: " + host);
            }
        }

        //get a string from a network stream
        public static string ReadString(NetworkStream stream, Encoding encoding)
        {
            // Check to see if this NetworkStream is readable.
            if (stream.CanRead)
            {
                byte[] readBuffer = new byte[1024];
                StringBuilder messageBuilder = new StringBuilder();
                int numberOfBytesRead = 0;

                // Incoming message may be larger than the buffer size.
                do
                {
                    numberOfBytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                    messageBuilder.AppendFormat("{0}", encoding.GetString(readBuffer, 0, numberOfBytesRead));

                }
                while (stream.DataAvailable);

                // return the received message
                return messageBuilder.ToString();
            }
            else
            {
                Console.WriteLine("Cannot read from this NetworkStream.");
                return "";
            }

        }

    }
}
