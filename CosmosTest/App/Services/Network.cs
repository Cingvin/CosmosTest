using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using CosmosTest.App.Model;
using System;
using System.Threading;

namespace CosmosTest.App.Services
{
    internal class Network
    {
        internal bool Initialize()
        {
            switch (Container.model.Mode)
            {
                case NetworkMode.DHCP:
                    return InitializeDHCP();
                case NetworkMode.Static:
                    return InitializeStatic();
                default:
                    return false;
            }
        }

        private bool InitializeStatic()
        {
            NetworkDevice nic = NetworkDevice.GetDeviceByName("eth0");
            bool answear= IPConfig.Enable(nic,
                new Address(Container.model.ip[0], Container.model.ip[1], Container.model.ip[2], Container.model.ip[3]),
                new Address(Container.model.subnet[0], Container.model.subnet[1], Container.model.subnet[2], Container.model.subnet[3]),
                new Address(Container.model.gateway[0], Container.model.gateway[1], Container.model.gateway[2], Container.model.gateway[3]));
            if (answear)
            {
                Container.console.ChangeStatus(0, "(static)"+ Container.model.ip[0]+"."+Container.model.ip[1] + "." + Container.model.ip[2] + "." + Container.model.ip[3]);
            }
            return answear;
        }

        private bool InitializeDHCP()
        {
            int attempts = 0;
            Container.console.ChangeStatus(0, "initializing dhcp");
            using (var xClient = new DHCPClient())
            {
                while (attempts < 5)
                {
                    if (xClient.SendDiscoverPacket() >= 0)
                    {
                        Container.console.ChangeStatus(0, "(dhcp)"+NetworkConfiguration.CurrentAddress.ToString());
                        return true;
                    }
                    Thread.Sleep(1000);
                    attempts++;
                    Container.console.ChangeStatus(0, "initializing dhcp: "+attempts+"s");
                }
            }
            Container.console.ChangeStatus(0, "failed",ConsoleColor.Red);
            return false;
        }
    }
}
