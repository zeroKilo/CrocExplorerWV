using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrocExplorerWV
{
    public class FileReference
    {
        public string name;
        public uint pos;
        public uint len;
        public uint ulen;
        public byte compression;

        public FileReference(string line)
        {
            string[] parts = line.Split(',');
            name = parts[0];
            pos = Convert.ToUInt32(parts[1]);
            len = Convert.ToUInt32(parts[2]);
            ulen = Convert.ToUInt32(parts[3]);
            switch (parts[4])
            {
                case "u":
                    compression = 0;
                    break;
                case "b":
                    compression = 1;
                    break;
                case "w":
                    compression = 2;
                    break;
            }
        }

        public override string ToString()
        {
            return name;
        }

        public string ToEntry()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name + ",");
            sb.Append(pos + ",");
            sb.Append(len + ",");
            sb.Append(ulen + ",");
            switch (compression)
            {
                case 1:
                    sb.Append("b");
                    break;
                case 2:
                    sb.Append("w");
                    break;
                default: 
                    sb.Append("u");
                    break;
            }
            sb.Append("\r\n");
            return sb.ToString();
        }
    }
}
