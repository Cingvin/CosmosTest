/*
* PROJECT:          Cosmos Operating System Development
* CONTENT:          FTP Server
* PROGRAMMERS:      Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Cosmos.System.FileSystem;

namespace CosmosFtpServer
{
    /// <summary>
    /// FTP data transfer mode.
    /// </summary>
    public enum TransferMode
    {
        NONE,
        ACTV,
        PASV
    }

    /// <summary>
    /// FTPCommand class.
    /// </summary>
    internal class FtpCommand
    {
        public string Command { get; set; }
        public string Content { get; set; }
    }

    /// <summary>
    /// FtpServer class. Used to handle FTP client connections.
    /// </summary>
    public class FtpServer : IDisposable
    {
        internal FtpCommandManager CommandManager { get; set; }
        internal TcpListener tcpListener;
        internal bool Listening;
        internal bool Debug;

        public FtpServer(CosmosVFS fs, string directory, bool debug = false)
        {
            if (Directory.Exists(directory) == false)
            {
                throw new Exception("FTP server can't open specified directory.");
            }

            IPAddress address = IPAddress.Any;
            tcpListener = new TcpListener(address, 21);
            CommandManager = new FtpCommandManager(fs, directory);
            Listening = true;
            Debug = debug;
        }

        /// <summary>
        /// Listen for new FTP clients.
        /// </summary>
        public void Listen()
        {
            tcpListener.Start();

            while (Listening)
            {
                try
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    IPEndPoint endpoint = client.Client.RemoteEndPoint as IPEndPoint;
                    Log("Client : New connection from " + endpoint.Address.ToString());
                    ReceiveNewClient(client);
                }
                catch (Exception ex)
                {
                    Log("Listener Exception: " + ex.Message);
                    Listening = false;
                }
            }
        }

        /// <summary>
        /// Handle new FTP client.
        /// </summary>
        private void ReceiveNewClient(TcpClient client)
        {
            var ftpClient = new FtpClient(client);
            ftpClient.SendReply(220, "Service ready for new user.");

            while (ftpClient.Control.Connected)
            {
                if (!ReceiveRequest(ftpClient))
                {
                    break;
                }
            }

            Log("Client session ended for " + (client.Client.RemoteEndPoint as IPEndPoint).Address.ToString());
            ftpClient.Control.Close();
        }

        /// <summary>
        /// Parse and execute FTP command.
        /// </summary>
        /// <returns>Returns false if the client disconnected.</returns>
        private bool ReceiveRequest(FtpClient ftpClient)
        {
            int bytesRead = 0;

            try
            {
                byte[] buffer = new byte[ftpClient.Control.ReceiveBufferSize];
                bytesRead = ftpClient.ControlStream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    return false;
                }

                string data = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();

                if (string.IsNullOrEmpty(data))
                {
                    return true;
                }

                data = data.TrimEnd(new char[] { '\r', '\n' });
                string[] splitted = data.Split(' ');
                FtpCommand command = new FtpCommand
                {
                    Command = splitted[0].ToUpperInvariant(),
                    Content = splitted.Length > 1 ? string.Join(" ", splitted.Skip(1)).Replace('/', '\\') : string.Empty
                };

                Log("Client : '" + command.Command + "' " + command.Content);
                CommandManager.ProcessRequest(ftpClient, command);
            }
            catch (Exception ex)
            {
                Log("ReceiveRequest Exception: " + ex.Message);
                return false;
            }

            return true;
        }

        private void Log(string str)
        {
            if (Debug)
            {
                Console.WriteLine(str);
                Cosmos.System.Global.Debugger.Send(str);
            }
        }

        public void Close()
        {
            Listening = false;
            tcpListener.Stop();
        }

        public void Dispose()
        {
            Close();
        }
    }
}