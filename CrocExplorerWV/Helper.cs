using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrocExplorerWV
{
    public static class Helper
    {
        public static uint ReadU32(Stream s)
        {
            ulong result = 0;
            result = (byte)s.ReadByte();
            result = (result << 8) | (byte)s.ReadByte();
            result = (result << 8) | (byte)s.ReadByte();
            result = (result << 8) | (byte)s.ReadByte();
            return (uint)result;
        }

        public static ushort ReadU16(Stream s)
        {
            ulong result = 0;
            result = (byte)s.ReadByte();
            result = (result << 8) | (byte)s.ReadByte();
            return (ushort)result;
        }
        
        public static byte[] CompressRLE(byte[] buff, byte type)
        {
            MemoryStream result = new MemoryStream();
            ushort last = 0;
            int start = 0;
            int skip = 1;
            if (type == 1)
            {
                for (int i = 0; i < buff.Length; i++)
                {
                    byte value = buff[i];
                    if (skip == 0 && i >= 2 &&
                        buff[i - 2] == buff[i - 1] &&
                        buff[i - 1] == buff[i])
                    {
                        byte c = (byte)(-(i - start - 2));
                        if (c != 0)
                        {
                            result.WriteByte(c);
                            result.Write(buff, start, i - start - 2);
                        }
                        int j = 0;
                        while (true)
                        {
                            if (i + j + 1>= buff.Length || j >= 127)
                                break;
                            byte value2 = buff[i + j + 1];
                            if (value2 != value)
                                break;
                            j++;
                        }
                        result.WriteByte((byte)j);
                        result.WriteByte((byte)last);
                        i += j;
                        start = i + 1;
                        skip = 2;
                    }
                    else
                    {
                        last = value;
                        if (skip > 0)
                            skip--;
                    }
                    if (i - start == 0x80)
                    {
                        result.WriteByte(0x80);
                        result.Write(buff, start, i - start);
                        start = i;
                        skip = 2;
                    }
                }
            }
            if (type == 2)
                for (int i = 0; i < buff.Length; i += 2)
                {
                    ushort value = (ushort)((buff[i] << 8) | buff[i + 1]);
                    if (skip == 0 && (value == last || i - start > 130))
                    {
                        byte c = (byte)(-(i - start - 2) / 2);
                        if (c != 0)
                        {
                            result.WriteByte(c);
                            result.Write(buff, start, i - start - 2);
                        }
                        int j = 0;
                        while (true)
                        {
                            if (i + j + 2 >= buff.Length || j >= 254)
                                break;
                            ushort value2 = (ushort)((buff[i + j + 2] << 8) | buff[i + j + 3]);
                            if (value2 != value)
                                break;
                            j += 2;
                        }
                        result.WriteByte((byte)(j / 2));
                        result.WriteByte((byte)(last >> 8));
                        result.WriteByte((byte)last);
                        i += j;
                        start = i + 2;
                        skip = 1;
                    }
                    else
                    {
                        last = value;
                        if (skip > 0)
                            skip--;
                    }
                }
            return result.ToArray();
        }

        public static byte[] DecompressRLE(byte[] buff, byte type)
        {
            MemoryStream result = new MemoryStream();
            if (type == 1)
            {
                for (int i = 0; i < buff.Length; i++)
                {
                    sbyte s = (sbyte)(buff[i]);
                    if (s < 0)
                    {
                        for (int j = 0; j < -s; j++)
                        {
                            i++;
                            result.WriteByte(buff[i]);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < s + 3; j++)
                            result.WriteByte(buff[i + 1]);
                        i++;
                    }
                }
            }
            if (type == 2)
            {
                for (int i = 0; i < buff.Length; i++)
                {
                    sbyte s = (sbyte)(buff[i]);
                    if (s < 0)
                    {
                        for (int j = 0; j < -s; j++)
                        {
                            i++;
                            result.WriteByte(buff[i]);
                            i++;
                            result.WriteByte(buff[i]);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < s + 2; j++)
                        {
                            result.WriteByte(buff[i + 1]);
                            result.WriteByte(buff[i + 2]);
                        }
                        i += 2;
                    }
                }
            }
            return result.ToArray();
        }

        public static void GetFileRefrence(TreeNode sel, out IdxFile idxFile, out FileReference fileRef)
        {
            idxFile = null;
            fileRef = null;
            bool found = false;
            if (sel != null && sel.Parent != null && sel.Parent.Text.EndsWith(".idx"))
            {
                string idxpath = "";
                if (sel.Parent.Parent == null)
                    idxpath = sel.Parent.Text;
                else
                    idxpath = sel.Parent.Parent.Text.Substring(1) + "\\" + sel.Parent.Text;
                foreach (IdxFile idx in FileSystem.idxFiles)
                {
                    if (idx.filename == idxpath)
                        foreach (FileReference r in idx.refs)
                            if (r.name == sel.Text)
                            {
                                found = true;
                                fileRef = r;
                                break;
                            }
                    if (found)
                    {
                        idxFile = idx;
                        break;
                    }
                }
            }
        }
    }
}
