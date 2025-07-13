using Cosmos.System.FileSystem;
using System;
using System.Linq;
namespace CosmosTest.App
{
    internal class FtpInitializer
    {
        private CosmosTest.App.FtpServer.FtpServer _server;

        internal FtpInitializer(StorageManager sm)
        {
#if DEBUG
            _server = new CosmosTest.App.FtpServer.FtpServer(sm.fs,sm.partitions.First().RootPath,true);
#else
             _server = new CosmosTest.App.FtpServer.FtpServer(sm.fs,sm.partitions.First().RootPath);
#endif
        }

        internal void Start()
        {
            _server.Listen();
        }
        internal void Stop()
        {
            _server.Close();
        }
    }
}