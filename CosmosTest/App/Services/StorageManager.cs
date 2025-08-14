using Cosmos.HAL.BlockDevice;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.FileSystem.VFS;
using System;
using System.Collections.Generic;

namespace CosmosTest.App.Services
{
    internal class StorageManager
    {
        internal CosmosVFS fs { get; private set; }
        internal List<ManagedPartition> partitions { get; private set; }
        internal StorageManager()
        {
            partitions = new List<ManagedPartition>();
            fs = new CosmosVFS();
            VFSManager.RegisterVFS(fs,true,false);
            List<Disk> disks = fs.GetDisks();
            for (int i = 0; i < disks.Count; i++)
            {
                Disk disk = disks[i];
                CheckDisk(disk, i);
            }
            if (partitions.Count == 0)
            {
                Container.console.RequestRestart("No partitions found!");
            }
            else
            {
                ListData();
            }
        }
        private void CheckDisk(Disk disk, int index)
        {
            try
            {
                if (disk.Type == BlockDeviceType.HardDrive)
                {
                    if (disk.Partitions.Count == 0)
                    {
                        long disksize = disk.Size;
                        if (disksize > 0 && disksize < int.MaxValue)
                        {
                            disk.CreatePartition(Convert.ToInt32(disksize));
                        }
                    }
                    for (int i = 0; i < disk.Partitions.Count; i++)
                    {
                        if (!AddPartition(disk.Partitions[i]))
                        {
                            disk.FormatPartition(i, "FAT32");
                            if (!AddPartition(disk.Partitions[i]))
                            {
                                Container.console.Error(index+"/"+(i + 1) + ". partition loading failed.");
                            }
                        }
                    }
                    disk.Mount();
                }
            }
            catch (Exception ex)
            {
                disk.Clear();
                Container.console.Error(ex.Message+" at disk "+index);
            }
        }
        private bool AddPartition(ManagedPartition partition)
        {
            try
            {
                string path = partition.RootPath;
                if (path.Length > 0 && fs.GetFileSystemType(path) == "FAT32" && partition.HasFileSystem)
                {
                    partitions.Add(partition);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Container.console.Error(ex);
            }
            return false;
        }
        public void ListData()
        {
            string message = "";
            foreach (ManagedPartition partition in partitions)
            {
                try
                {
                    message += partition.RootPath + " size:" + Convert.ToString(fs.GetAvailableFreeSpace(partition.RootPath)) + " byte\r\n";
                }catch(Exception ex)
                {
                    Container.console.Error(ex);
                }
            }
            Container.console.Message(new Model.Message(message));
            Container.console.ChangeStatus(1, Convert.ToString(partitions.Count));
        }
    }
}