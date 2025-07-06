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
        private CosmosVFS fs;
        protected override void OnBoot()
        {
            base.OnBoot();
            Console.Clear();
            Console.WriteLine("\r\n\r\n ________   ________  ________      \r\n|\\   ___  \\|\\   __  \\|\\   ____\\     \r\n\\ \\  \\\\ \\  \\ \\  \\|\\  \\ \\  \\___|_    \r\n \\ \\  \\\\ \\  \\ \\   __  \\ \\_____  \\   \r\n  \\ \\  \\\\ \\  \\ \\  \\ \\  \\|____|\\  \\  \r\n   \\ \\__\\\\ \\__\\ \\__\\ \\__\\____\\_\\  \\ \r\n    \\|__| \\|__|\\|__|\\|__|\\_________\\\r\n                        \\|_________|\r\n\r\n");
        }
        protected override void BeforeRun()
        {
            if (!Network.Initialize())
            {
                Console.WriteLine("Press any key to restart");
                Console.ReadKey();
                Cosmos.System.Power.Reboot();
            }
            this.fs = new CosmosVFS();
            VFSManager.RegisterVFS(this.fs,true,true);
            try
            {
                this.application = new Application(this.fs);
                Console.WriteLine("Application started");
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!! HIBA AZ ALKALMAZAS LETREHOZASAKOR !!!");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Press any key to restart");
                Console.ReadKey();
                Sys.Power.Reboot();
            }
        }
        protected override void Run()
        {
            while (true)
            {
                this.application.Run();
                Console.WriteLine("niga");
            }
        }
    }
}