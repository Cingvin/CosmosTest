using System;
using System.Linq;

namespace CosmosTest.App.Model
{
    internal class NasModel
    {
        internal NetworkMode Mode { get; set; } = NetworkMode.DHCP;
        internal byte[] ip { get; set; } = new byte[] { 0, 0, 0, 0 };
        internal byte[] subnet { get; set; } = new byte[] { 0, 0, 0, 0 };
        internal byte[] gateway { get; set; } = new byte[] { 0, 0, 0, 0 };
        internal bool ftp { get; set; } = false;
        internal bool http { get; set; } = false;

        internal void Deserialize(string json) {
            string[] lines = json.Split(",\n\t");
            int mode=Convert.ToInt32(lines[0].Split(':')[1]);
            this.Mode = (NetworkMode)mode;
            this.ip = DeserializeByteArray(lines[1].Split(':')[1]);
            this.subnet = DeserializeByteArray(lines[2].Split(':')[1]);
            this.gateway = DeserializeByteArray(lines[3].Split(':')[1]);
            this.ftp = DeserializeBoolean(lines[4].Split(':')[1]);
            this.http = DeserializeBoolean(lines[5].Split(':')[1]);
        }
        public override string ToString()
        {
            return "{"
                        +"\n\tMode:"+(int)Mode+","
                        +"\n\tip:[" + ip[0] + "," + ip[1] + "," + ip[2] + "," + ip[3] + "],"
                        +"\n\tsubnet:[" + subnet[0] + "," + subnet[1] + "," + subnet[2] + "," + subnet[3] + "],"
                        +"\n\tgateway:[" + gateway[0] + "," + gateway[1] + "," + gateway[2] + "," + gateway[3] + "],"
                        +"\n\tftp:" + (ftp?"true":"false") + ","
                        +"\n\thttp:" + (http?"true":"false")
                + "\n}";
        }
        private bool DeserializeBoolean(string json)
        {
            if(json.Contains("true"))
                return true;
            return false;
        }
        private byte[] DeserializeByteArray(string json)
        {
            string trimmed=new string(json.Skip(1).Take(json.Length - 2).ToArray());
            string[] array = trimmed.Split(",");
            byte[] bytes = new byte[array.Length];
            for(int i=0;i<array.Length;i++)
            {
                bytes[i]=Convert.ToByte(array[i]);
            }
            return bytes;
        }
    }
}
