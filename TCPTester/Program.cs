using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace TCPTester
{
    class Program
    {
        static void Main(string[] args)
        {
            string output = "";

            try
            {
                // Create a TcpClient. 
                // The client requires a TcpServer that is connected 
                // to the same address specified by the server and port 
                // combination
                Console.WriteLine("puerto:");
                int port = Int32.Parse(Console.ReadLine());
                port = 3033;
                Console.Write("Escribir IP a conectarse:");
                string ip = Console.ReadLine(); ;
                ip = "192.168.1.7";
                TcpClient client = new TcpClient(ip, port);
                Console.WriteLine("escribir mensaje:");
                string message = Console.ReadLine();
                message = ":㄀4`3㄀ ㄀㄀㄀㄀㄀㄀㄀㄀㄀01002000000000000006010000016USUARIO/PASSWORD015255.254.253.251";

                byte[] bytes = new byte[256];
                // Translate the passed message into ASCII and store it as a byte array.
                Byte[] data = new Byte[256];
                data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing. 
                // Stream stream = client.GetStream();
                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                output = "Sent: " + message;
                Console.WriteLine(output);

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes23 = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes23);
                output = "Received: " + responseData;
                Console.WriteLine(output);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (SocketException e)
            {
                output = "SocketException: " + e.ToString();
                Console.WriteLine(output);
            }
            catch (Exception ex)
            {
                output = "SocketException: " + ex.ToString();
                Console.WriteLine(output);
            }

            Console.ReadLine();
        }
    }
}
