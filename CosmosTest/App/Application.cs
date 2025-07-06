using Cosmos.System.FileSystem;
using System;
namespace CosmosTest.App
{
    internal class Application
    {
        private FtpInitializer ftp;
        private CosmosVFS fs = null!;

        internal Application(CosmosVFS fs)
        {
            this.fs = fs;
            this.ftp = new FtpInitializer(fs);
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