using Cosmos.System.FileSystem;
using System;
namespace CosmosTest.App
{
    internal class FtpInitializer
    {
        private CosmosTest.App.FtpServer.FtpServer _server;

        internal FtpInitializer(CosmosVFS fs)
        {
#if DEBUG
            _server = new CosmosTest.App.FtpServer.FtpServer(fs, "0:\\",true);
#else
            _server = new CosmosTest.App.FtpServer.FtpServer(fs, "0:\\");
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