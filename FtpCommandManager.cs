using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.Network.Config;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CosmosFtpServer
{
    internal class FtpCommandManager
    {
        internal CosmosVFS FileSystem { get; set; }
        internal string BaseDirectory { get; set; }
        internal string CurrentDirectory { get; set; }

        internal FtpCommandManager(CosmosVFS fs, string directory)
        {
            FileSystem = fs;
            CurrentDirectory = directory;
            BaseDirectory = directory;
        }

        private void CloseDataConnection(FtpClient ftpClient)
        {
            ftpClient.DataStream?.Close();
            ftpClient.Data?.Close();
            if (ftpClient.Mode == TransferMode.PASV)
            {
                ftpClient.DataListener?.Stop();
            }
            ftpClient.Mode = TransferMode.NONE;
        }

        internal void ProcessRequest(FtpClient ftpClient, FtpCommand command)
        {
            if (command.Command == "USER") { ProcessUser(ftpClient, command); }
            else if (command.Command == "PASS") { ProcessPass(ftpClient, command); }
            else
            {
                if (ftpClient.IsConnected())
                {
                    switch (command.Command.ToUpper())
                    {
                        case "CWD": ProcessCwd(ftpClient, command); break;
                        case "SYST": ftpClient.SendReply(215, "CosmosOS"); break;
                        case "CDUP": ProcessCdup(ftpClient, command); break;
                        case "QUIT": ProcessQuit(ftpClient, command); break;
                        case "DELE": ProcessDele(ftpClient, command); break;
                        case "PWD": ProcessPwd(ftpClient, command); break;
                        case "PASV": ProcessPasv(ftpClient, command); break;
                        case "PORT": ProcessPort(ftpClient, command); break;
                        case "HELP": ftpClient.SendReply(200, "Help done."); break;
                        case "NOOP": ftpClient.SendReply(200, "Command okay."); break;
                        case "FEAT": ftpClient.SendReply(500, "Unknown command."); break;
                        case "RETR": ProcessRetr(ftpClient, command); break;
                        case "STOR": ProcessStor(ftpClient, command); break;
                        case "RMD": ProcessRmd(ftpClient, command); break;
                        case "MKD": ProcessMkd(ftpClient, command); break;
                        case "LIST": ProcessList(ftpClient, command); break;
                        case "TYPE": ftpClient.SendReply(200, "Command okay."); break;
                        default: ftpClient.SendReply(500, "Unknown command."); break;
                    }
                }
            }
        }

        internal void ProcessUser(FtpClient ftpClient, FtpCommand command)
        {
            if (string.IsNullOrEmpty(command.Content)) { ftpClient.SendReply(501, "Syntax error in parameters or arguments."); return; }
            ftpClient.Username = command.Content;
            ftpClient.SendReply(331, "User name okay, need password.");
        }

        internal void ProcessPass(FtpClient ftpClient, FtpCommand command)
        {
            ftpClient.Password = command.Content;
            ftpClient.Connected = true;
            ftpClient.LastGoodDirectory = BaseDirectory;
            ftpClient.SendReply(230, "User logged in, proceed.");
        }

        internal void ProcessCwd(FtpClient ftpClient, FtpCommand command)
        {
            if (string.IsNullOrEmpty(command.Content)) { ftpClient.SendReply(501, "Syntax error in parameters or arguments."); return; }
            try
            {
                string targetDir;
                if (command.Content.StartsWith("/") || command.Content.StartsWith("\\"))
                {
                    targetDir = Path.Combine(BaseDirectory, command.Content.TrimStart('/', '\\'));
                }
                else
                {
                    targetDir = Path.Combine(ftpClient.LastGoodDirectory, command.Content);
                }

                if (Directory.Exists(targetDir))
                {
                    ftpClient.LastGoodDirectory = targetDir;
                    ftpClient.SendReply(250, "Requested file action okay, directory changed.");
                }
                else
                {
                    ftpClient.SendReply(550, "Requested action not taken. Directory not found.");
                }
            }
            catch { ftpClient.SendReply(550, "Requested action not taken."); }
        }

        internal void ProcessPwd(FtpClient ftpClient, FtpCommand command)
        {
            string current = ftpClient.LastGoodDirectory.Replace(BaseDirectory, "").Replace('\\', '/');
            if (string.IsNullOrEmpty(current)) { current = "/"; }
            ftpClient.SendReply(257, $"\"{current}\" is the current directory.");
        }

        internal void ProcessPasv(FtpClient ftpClient, FtpCommand command)
        {
            try
            {
                Console.WriteLine("Processing PASV...");
                ushort port = Cosmos.System.Network.IPv4.TCP.Tcp.GetDynamicPort();
                Console.WriteLine("Got dynamic port: " + port);

                var cosmosAddress = NetworkConfiguration.CurrentAddress;

                var netAddress = System.Net.IPAddress.Parse(cosmosAddress.ToString());

                ftpClient.DataListener = new TcpListener(netAddress, port);
                Console.WriteLine("TCP Listener created for " + netAddress.ToString() + ":" + port);

                ftpClient.DataListener.Start();
                Console.WriteLine("Listener started.");

                var addressBytes = cosmosAddress.ToByteArray();
                byte p1 = (byte)(port / 256);
                byte p2 = (byte)(port % 256);
                string pasvReplyMessage = string.Format("Entering Passive Mode ({0},{1},{2},{3},{4},{5})", addressBytes[0], addressBytes[1], addressBytes[2], addressBytes[3], p1, p2);

                Console.WriteLine("Sending reply: " + pasvReplyMessage);
                ftpClient.SendReply(227, pasvReplyMessage);
                ftpClient.Mode = TransferMode.PASV;
            }
            catch (Exception e)
            {
                Console.WriteLine("!!!! PASV FAILED !!!!");
                Console.WriteLine(e.ToString());
                ftpClient.SendReply(425, "Can't open data connection.");
            }
        }

        internal void ProcessPort(FtpClient ftpClient, FtpCommand command)
        {
            string[] splitted = command.Content.Split(',');
            byte[] array = new byte[] { (byte)int.Parse(splitted[0]), (byte)int.Parse(splitted[1]), (byte)int.Parse(splitted[2]), (byte)int.Parse(splitted[3]) };
            IPAddress address = new IPAddress(array);
            ftpClient.Data = new TcpClient();
            ftpClient.Address = address;
            ftpClient.Port = int.Parse(splitted[4]) * 256 + int.Parse(splitted[5]);
            ftpClient.SendReply(200, "Entering Active Mode.");
            ftpClient.Mode = TransferMode.ACTV;
        }

        internal void ProcessList(FtpClient ftpClient, FtpCommand command)
        {
            try
            {
                if (ftpClient.Mode == TransferMode.NONE) { ftpClient.SendReply(425, "Use PASV or PORT first."); return; }
                if (ftpClient.Mode == TransferMode.PASV) { ftpClient.Data = ftpClient.DataListener.AcceptTcpClient(); }
                else { ftpClient.Data.Connect(ftpClient.Address, ftpClient.Port); }

                ftpClient.DataStream = ftpClient.Data.GetStream();
                ftpClient.SendReply(150, "Here comes the directory listing.");

                var directory_list = FileSystem.GetDirectoryListing(ftpClient.LastGoodDirectory);
                var sb = new StringBuilder();
                foreach (var directoryEntry in directory_list)
                {
                    if (directoryEntry.mEntryType == DirectoryEntryTypeEnum.Directory) { sb.Append("d"); } else { sb.Append("-"); }
                    sb.Append("rwxrwxrwx 1 ftp ftp ");
                    sb.Append(directoryEntry.mSize.ToString().PadLeft(12));
                    sb.Append(" Jun 30 16:08 ");
                    sb.AppendLine(directoryEntry.mName);
                }

                byte[] listData = Encoding.ASCII.GetBytes(sb.ToString());
                ftpClient.DataStream.Write(listData, 0, listData.Length);

                ftpClient.SendReply(226, "Directory send OK.");
            }
            catch (Exception e) { Console.WriteLine("LIST Error: " + e.ToString()); ftpClient.SendReply(425, "Can't open data connection."); }
            finally
            {
                CloseDataConnection(ftpClient);
            }
        }

        internal void ProcessStor(FtpClient ftpClient, FtpCommand command)
        {
            if (string.IsNullOrEmpty(command.Content)) { ftpClient.SendReply(501, "Syntax error."); return; }
            try
            {
                if (ftpClient.Mode == TransferMode.NONE) { ftpClient.SendReply(425, "Use PASV or PORT first."); return; }
                if (ftpClient.Mode == TransferMode.PASV) { ftpClient.Data = ftpClient.DataListener.AcceptTcpClient(); }
                else { ftpClient.Data.Connect(ftpClient.Address, ftpClient.Port); }

                ftpClient.DataStream = ftpClient.Data.GetStream();
                ftpClient.SendReply(150, "Opening data connection for file transfer.");

                string filePath = Path.Combine(ftpClient.LastGoodDirectory, command.Content);
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    byte[] buffer = new byte[8192];
                    int count;
                    while ((count = ftpClient.DataStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, count);
                    }
                }
                ftpClient.SendReply(226, "Transfer complete.");
            }
            catch (Exception e) { Console.WriteLine("STOR Error: " + e.Message); ftpClient.SendReply(425, "Can't open data connection."); }
            finally
            {
                CloseDataConnection(ftpClient);
            }
        }

        internal void ProcessRetr(FtpClient ftpClient, FtpCommand command)
        {
            if (string.IsNullOrEmpty(command.Content)) { ftpClient.SendReply(501, "Syntax error."); return; }
            try
            {
                string filePath = Path.Combine(ftpClient.LastGoodDirectory, command.Content);
                if (!File.Exists(filePath)) { ftpClient.SendReply(550, "File not found."); return; }
                if (ftpClient.Mode == TransferMode.NONE) { ftpClient.SendReply(425, "Use PASV or PORT first."); return; }
                if (ftpClient.Mode == TransferMode.PASV) { ftpClient.Data = ftpClient.DataListener.AcceptTcpClient(); }
                else { ftpClient.Data.Connect(ftpClient.Address, ftpClient.Port); }

                ftpClient.DataStream = ftpClient.Data.GetStream();
                ftpClient.SendReply(150, "Opening data connection for file transfer.");

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[8192];
                    int count;
                    while ((count = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ftpClient.DataStream.Write(buffer, 0, count);
                    }
                }
                ftpClient.SendReply(226, "Transfer complete.");
            }
            catch (Exception e) { Console.WriteLine("RETR Error: " + e.Message); ftpClient.SendReply(425, "Can't open data connection."); }
            finally
            {
                CloseDataConnection(ftpClient);
            }
        }

        internal void ProcessDele(FtpClient ftpClient, FtpCommand command) { ftpClient.SendReply(550, "Not implemented."); }
        internal void ProcessRmd(FtpClient ftpClient, FtpCommand command) { ftpClient.SendReply(550, "Not implemented."); }
        internal void ProcessMkd(FtpClient ftpClient, FtpCommand command) { ftpClient.SendReply(550, "Not implemented."); }
        internal void ProcessCdup(FtpClient ftpClient, FtpCommand command) { ftpClient.SendReply(550, "Not implemented."); }
        internal void ProcessQuit(FtpClient ftpClient, FtpCommand command)
        {
            ftpClient.SendReply(221, "Goodbye.");
            ftpClient.Control.Close();
        }
    }
}