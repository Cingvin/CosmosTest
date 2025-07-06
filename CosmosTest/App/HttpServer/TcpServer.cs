using Cosmos.System.Network.Config;
using CosmosTest.Resources;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CosmosTest.App.HttpServer
{
    internal class TcpServer
    {
        private TcpListener tcpListener;
        private int port;

        public TcpServer(int port)
        {
            this.port = port;
            var address = new IPAddress(NetworkConfiguration.CurrentAddress.ToByteArray());
            this.tcpListener = new TcpListener(address, port);

            this.tcpListener.Start();
            Console.WriteLine("HTTP Server listening on " + NetworkConfiguration.CurrentAddress.ToString() + ":" + port);
        }

        public void Poll()
        {
            if (this.tcpListener.Pending())
            {
                TcpClient client = this.tcpListener.AcceptTcpClient();
                HandleClientComm(client);
                client.Close();
            }
        }

        private void HandleClientComm(TcpClient client)
        {
            string content = Pages.index;
            byte[] body = Encoding.UTF8.GetBytes(content);

            string header = "HTTP/1.1 200 OK\r\n" +
                            "Content-Type: text/html\r\n" +
                            "Content-Length: " + body.Length + "\r\n" +
                            "\r\n";

            byte[] headerBytes = Encoding.ASCII.GetBytes(header);
            byte[] response = new byte[headerBytes.Length + body.Length];

            Buffer.BlockCopy(headerBytes, 0, response, 0, headerBytes.Length);
            Buffer.BlockCopy(body, 0, response, headerBytes.Length, body.Length);

            NetworkStream stream = client.GetStream();
            stream.Write(response, 0, response.Length);
            stream.Close();
        }
    }
}