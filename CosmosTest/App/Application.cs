using Cosmos.System.FileSystem;
using System;
namespace CosmosTest.App
{
    internal class Application
    {
        private FtpInitializer ftp;
        private StorageManager sm = null!;

        internal Application(StorageManager sm)
        {
            this.sm = sm;
            this.ftp = new FtpInitializer(sm);
        }
        internal void Run()
        {
            ftp.Start();
        }
        internal void Stop()
        {
            ftp.Stop();
        }
    }
}