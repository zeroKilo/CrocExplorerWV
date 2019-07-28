using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrocExplorerWV
{
    public class IdxFile
    {
        public List<FileReference> refs = new List<FileReference>();
        public string basepath;
        public string filename;
        public string wadname;
        public IdxFile(string path, int basePathLen)
        {
            basepath = path.Substring(0, basePathLen);
            filename = path.Substring(basePathLen);
            wadname = filename.Substring(0, filename.Length - 3) + "wad";
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
                refs.Add(new FileReference(line));
        }

        public byte[] LoadEntry(FileReference r)
        {
            FileStream fs = new FileStream(basepath + wadname, FileMode.Open, FileAccess.Read);
            fs.Seek(r.pos, 0);
            byte[] buff = new byte[r.len];
            fs.Read(buff, 0, (int)r.len);
            fs.Close();
            if (r.compression != 0)
                buff = Helper.DecompressRLE(buff, r.compression);
            if (buff.Length > r.ulen)
            {
                MemoryStream m = new MemoryStream();
                m.Write(buff, 0, (int)r.ulen);
                buff = m.ToArray();
            }
            return buff;
        }

        public void SaveEntry(FileReference r, byte[] data, uint ucsize)
        {
            byte[] before = File.ReadAllBytes(basepath + wadname);
            MemoryStream m = new MemoryStream();
            StringBuilder result = new StringBuilder();
            uint pos = 0;
            foreach(FileReference fr in refs)
                if (fr.name != r.name)
                {
                    m.Write(before, (int)fr.pos, (int)fr.len);
                    fr.pos = pos;
                    result.Append(fr.ToEntry());
                    pos += fr.len;
                }
                else
                {
                    m.Write(data, 0, data.Length);
                    fr.pos = pos;
                    fr.len = (uint)data.Length;
                    fr.ulen = ucsize;
                    result.Append(fr.ToEntry());
                    pos += (uint)data.Length;
                }
            File.WriteAllText(basepath + filename, result.ToString());
            File.WriteAllBytes(basepath + wadname, m.ToArray());
            Log.WriteLine(filename + " updated.");
        }

        public TreeNode ToTree()
        {
            TreeNode result;
            TreeNode current;
            if (filename.Contains("\\"))
            {
                string[] parts = filename.Split('\\');
                result = new TreeNode("\\" + parts[0]);
                current = new TreeNode(parts[1]);
                result.Nodes.Add(current);
            }
            else
            {
                result = new TreeNode(filename);
                current = result;
            }
            foreach (FileReference r in refs)
                current.Nodes.Add(r.name);
            return result;
        }
    }
}
