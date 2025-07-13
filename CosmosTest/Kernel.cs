using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using CosmosTest.App;
using System;
using System.IO;
using Sys = Cosmos.System;

namespace CosmosTest
{
    public class Kernel : Sys.Kernel
    {
        private Application application;
        private StorageManager storage;
        protected override void OnBoot()
        {
            base.OnBoot();
            Console.Clear();
            Console.WriteLine("\r\n\r\n ________   ________  ________      \r\n|\\   ___  \\|\\   __  \\|\\   ____\\     \r\n\\ \\  \\\\ \\  \\ \\  \\|\\  \\ \\  \\___|_    \r\n \\ \\  \\\\ \\  \\ \\   __  \\ \\_____  \\   \r\n  \\ \\  \\\\ \\  \\ \\  \\ \\  \\|____|\\  \\  \r\n   \\ \\__\\\\ \\__\\ \\__\\ \\__\\____\\_\\  \\ \r\n    \\|__| \\|__|\\|__|\\|__|\\_________\\\r\n                        \\|_________|\r\n\r\n");
        }
        protected override void BeforeRun()
        {
            try
            {
                GCImplementation.Init();
                if (!Network.Initialize())
                {
                    ConsoleManager.RequestRestart();
                }
                this.storage = new StorageManager();
                this.application = new Application(this.storage);
                ConsoleManager.Log("Application started");
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!! HIBA AZ ALKALMAZAS LETREHOZASAKOR !!!");
                ConsoleManager.Error(ex);
                ConsoleManager.RequestRestart();
            }
        }
        protected override void Run()
        {
            while (true)
            {
                try
                {
                    this.application.Run();
                }
                catch (Exception ex)
                {
                    ConsoleManager.Error(ex);
                }
            }
        }
    }
}