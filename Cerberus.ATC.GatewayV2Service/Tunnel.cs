using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Cerberus.ATC.GatewayV2Service
{
    public class Tunnel
    {
        public TcpListener tcpListener = null;
        const int sizeBuffer = 256;
        public NetworkStream stream { get; set; }
        public TcpClient tcpClient { get; set; }

        public byte[] byteReceived { get; set; }
        public Action<Tunnel> ReleaseTunnel { get; set; }

        public string IdTunnel { get; set; }
        public bool IsAvailable { get; set; }

        public static string Get_ip_local_address()
        {
            System.Net.IPHostEntry host;
            string localIP = "?";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            return localIP;
        }

        public Tunnel(int puerto)
        {
            IPAddress ipAddress = IPAddress.Parse(Get_ip_local_address());
            tcpListener = new TcpListener(ipAddress, puerto);
            tcpListener.ExclusiveAddressUse = false;
        }

        public void TryOpen()
        {
            tcpListener.Start();
        }

        public TcpClient ConnectedClient()
        {
            tcpListener.Start();
            tcpClient = tcpListener.AcceptTcpClient();
            tcpClient.NoDelay = true;
            byteReceived = new byte[sizeBuffer];
            stream = tcpClient.GetStream();
            stream.Read(byteReceived, 0, byteReceived.Length);
            //TODO: aca debo leer el nemotico del buffer byteReceived
            string nemotic = "BISA";
            //return nemotic;
            return tcpClient;
        }

        public void SendMessage(string message)
        {
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] byteConvert = asen.GetBytes(message);
            stream.Write(byteConvert, 0, byteConvert.Length);
        }

        public void SendMessage(byte[] message)
        {
            stream.Write(message, 0, message.Length);
        }

        public string ReadMessage()
        {
            byte[] b = new byte[sizeBuffer];
            stream = tcpClient.GetStream();
            stream.Read(b, 0, b.Length);
            string resul = System.Text.Encoding.UTF8.GetString(b); ;
            return resul;
        }

        public byte[] ReadMessageByte()
        {
            byte[] b = new byte[sizeBuffer];
            stream = tcpClient.GetStream();
            stream.Read(b, 0, b.Length);
            return b;
        }

        public void Close()
        {
            stream.Flush();
            tcpClient.Close();
        }


    }
}
