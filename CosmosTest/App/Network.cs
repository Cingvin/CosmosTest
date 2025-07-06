using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using System;
using System.Threading;

namespace CosmosTest.App
{
    internal static class Network
    {
        internal static bool Initialize()
        {
            int attempts = 0;
            Console.Write("Network Initializing");
            using (var xClient = new DHCPClient())
            {
                while (attempts < 5)
                {
                    if (xClient.SendDiscoverPacket() >= 0)
                    {
                        Console.WriteLine(" done");
                        Console.WriteLine("IP-Address: " + NetworkConfiguration.CurrentAddress.ToString());
                        return true;
                    }
                    Thread.Sleep(1000);
                    Console.Write(".");
                    attempts++;
                }
            }
            Console.WriteLine("failed");
            return false;
        }
    }
}
