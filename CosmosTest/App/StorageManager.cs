using Cosmos.HAL.BlockDevice;
using Cosmos.HAL.BlockDevice.Registers;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.FAT;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CosmosTest.App
{
    internal class StorageManager
    {
        internal CosmosVFS fs { get; private set; }
        private FatFileSystemFactory factory;
        internal List<ManagedPartition> partitions;
        internal StorageManager()
        {
            fs = new CosmosVFS();
            VFSManager.RegisterVFS(fs);
            fs.Initialize(false);
            List<Disk> disks = fs.GetDisks();
            factory = new FatFileSystemFactory();
            ConsoleManager.Log(disks.Count + " disk found on this device");
            for (int i = 0; i < disks.Count; i++)
            {
                ConsoleManager.Log((i + 1) + ". disk scan");
                Disk disk = disks[i];
                CheckDisk(disk, i);
            }
            partitions = new List<ManagedPartition>();
        }
        private void CheckDisk(Disk disk, int index)
        {
            try
            {
                if (disk.Type == BlockDeviceType.HardDrive)
                {
                    disk.Mount();
                    if (disk.Partitions.Count == 0)
                    {
                        ConsoleManager.Log("Createing partition");
                        long disksize = disk.Size;
                        if (disksize > 0 && disksize < Int32.MaxValue)
                        {
                            disk.CreatePartition(Convert.ToInt32(disksize));
                            disk.Partitions.Last().RootPath = (index + 1) + ":\\";
                            ConsoleManager.Log("Partition created");
                        }
                        else
                        {
                            ConsoleManager.Log("Invalid Disk size:" + disksize);
                        }
                    }
                    ConsoleManager.Log("Found " + disk.Partitions.Count + " partition");
                    for (int i = 0; i < disk.Partitions.Count; i++)
                    {
                        if (AddPartition(disk.Partitions[i]))
                        {
                            disk.DeletePartition(i);
                            i--;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                disk.Clear();
                ConsoleManager.Error(ex);
            }
        }
        private bool AddPartition(ManagedPartition partition)
        {
            try
            {
                partitions.Add(partition);
                ConsoleManager.Log(partition.RootPath + " loaded");
                return true;
            }
            catch (Exception ex)
            {
                ConsoleManager.Error(ex);
                return false;
            }
        }
    }
}