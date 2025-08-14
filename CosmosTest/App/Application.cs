using Cosmos.System.FileSystem;
using CosmosTest.App.Model;
using CosmosTest.App.Services;
using System;
using System.IO;
using System.Linq;
namespace CosmosTest.App
{
    internal class Application
    {
        private FtpServer.FtpServer ftpServer;
        private Task ftp;
        private TaskManager manager;
        internal Application()
        {
            ManagedPartition partition = Container.storage.partitions.FirstOrDefault();
            if (partition != null)
            {
#if DEBUG
                ftpServer = new FtpServer.FtpServer(Container.storage.fs, partition.RootPath, true);
#else
                ftpServer = new CosmosTest.App.FtpServer.FtpServer(Container.storage.fs,partition.RootPath);
#endif
            }
            manager = new TaskManager();
            manager.Add(new Task(() => WaitForCommand()));
            if (Container.model.ftp)
            {
                StartFtp();
            }
            if(Container.model.http)
            {
                StartHttp();
            }
            manager.Run();
        }
        internal static void Initialize()
        {
            Container.console = new ConsoleManager(ConsoleColor.White, ConsoleColor.DarkBlue);
            Container.storage = new StorageManager();
            if (File.Exists("0:\\NasData\\model.json"))
            {
                try
                {
                    string text = File.ReadAllText("0:\\NasData\\model.json");
                    Container.model = new NasModel();
                    Container.model.Deserialize(text);
                }
                catch (Exception ex)
                {
                    Container.console.Error(ex);
                    Container.model = new NasModel();
                }
            }
            else
            {
                Container.model = new NasModel();
            }
            Container.network = new Network();
            if (!Container.network.Initialize())
                Container.console.Error("Network initializing error.");
            Container.console.Message(ListCommand());
        }
        private void WaitForCommand()
        {
            //while (true)
            //{
                try
                {
                    string rawline = Console.ReadLine();
                    string[] line = rawline.Split(' ');
                    switch (line[0])
                    {
                        case "restart":
                            Cosmos.System.Power.Reboot();
                            break;
                        case "shutdown":
                            Cosmos.System.Power.Shutdown();
                            break;
                        case "enableftp":
                            StartFtp();
                            break;
                        case "disableftp":
                            StopFtp();
                            break;
                        case "enablehttp":
                            StartHttp();
                            break;
                        case "disablehttp":
                            StopHttp();
                            break;
                        case "network":
                            Network(line[1]);
                            break;
                        case "set":
                            Set(line);
                            break;
                        case "save":
                            Save();
                            break;
                        default:
                            Container.console.Message(ListCommand());
                            break;
                    }
                    Container.console.Message(">" + rawline);
                }
                catch (Exception ex)
                {
                    Container.console.Error(ex);
                }
            //}
        }
        #region commands
        private static string ListCommand()
        {
            return "Commands:\n" +
                   "restart:     restart the computer\n" +
                   "shutdown:    shutdown the computer\n" +
                   "save:        save the current settings as startup config\n" +
                   "enableftp:   enable the ftp ftpServer\n" +
                   "disableftp:  disable the ftp ftpServer\n" +
                   "enablehttp:  enable the http ftpServer\n" +
                   "disablehttp: disable the http ftpServer\n" +
                   "network:     change network mode with dhcp/static command\n" +
                   "set:         set ip/subnet/gateway masks with xxx.xxx.xxx.xxx formula";
        }
        private void StartFtp()
        {
            if (ftpServer != null)
            {
                Container.model.ftp = true;
                Container.console.ChangeStatus(2, "active", ConsoleColor.Green);
                //ftp = new Task(() => ftpServer.Listen());
                //manager.Add(ftp);
                ftpServer.Listen();
            }
        }
        private void StopFtp()
        {
            if (ftpServer != null)
            {
                Container.model.ftp = false;
                Container.console.ChangeStatus(2, "inactive", ConsoleColor.Red);
                manager.Remove(ftp);
                ftpServer.Close();
            }
        }
        private void StartHttp()
        {
            if (true)
            {
                Container.model.http = true;
                Container.console.ChangeStatus(3, "active", ConsoleColor.Green);
            }
        }
        private void StopHttp()
        {
            if (true)
            {
                Container.model.http = false;
                Container.console.ChangeStatus(3, "inactive", ConsoleColor.Red);
            }
        }
        private void Network(string command)
        {
            switch (command)
            {
                case "dhcp":
                    Container.model.Mode = NetworkMode.DHCP;
                    break;
                case "static":
                    Container.model.Mode = NetworkMode.Static;
                    break;
            }
        }
        private void Set(string[] command)
        {
            switch (command[1])
            {
                case "ip":
                    Container.model.ip = ProcessArray(command[2]);
                    break;
                case "gateway":
                    Container.model.gateway = ProcessArray(command[2]);
                    break;
                case "subnet":
                    Container.model.subnet = ProcessArray(command[2]);
                    break;
            }
        }
        private byte[] ProcessArray(string array)
        {
            string[] bytes = array.Split('.');
            byte[] bytes1 = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes1[i] = Convert.ToByte(bytes[i]);
            }
            return bytes1;
        }
        private void Save()
        {
            if(!Directory.Exists("0:\\NasData"))
            {
                Directory.CreateDirectory("0:\\NasData");
            }
            if(!File.Exists("0:\\NasData\\model.json"))
            {
                File.Create("0:\\NasData\\model.json");
            }
            File.WriteAllText("0:\\NasData\\model.json", Container.model.ToString());
        }
        #endregion
    }
}