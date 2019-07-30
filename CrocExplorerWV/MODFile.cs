using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrocExplorerWV
{
    public class MODFile
    {
        public List<MODObject> obj = new List<MODObject>();
        public MODFile(byte[] data)
        {
            MemoryStream m = new MemoryStream(data);
            m.Seek(0, 0);
            ushort count = Helper.ReadU16BE(m);
            ushort flags = Helper.ReadU16BE(m);
            for (int i = 0; i < count; i++)
                obj.Add(new MODObject(m));
        }

        public void SaveToObj(string path)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;
            int vcount = 0;
            foreach (MODObject o in obj)
                sb.Append(o.SaveToObj(count++, ref vcount));
            File.WriteAllText(path, sb.ToString());
        }

        public class MODObject
        {
            public int radius;
            public MODBBox[] bbox = new MODBBox[9];
            public uint countVerts;
            public MODVector3[] vertices;
            public MODVector3[] normals;
            public uint countFaces;
            public MODFace[] faces;

            public MODObject(Stream s)
            {
                radius = (int)Helper.ReadU32BE(s);
                for (int i = 0; i < 9; i++)
                    bbox[i] = new MODBBox(s);
                countVerts = Helper.ReadU32BE(s);
                vertices = new MODVector3[countVerts];
                normals = new MODVector3[countVerts];
                for (int i = 0; i < countVerts; i++)
                    vertices[i] = new MODVector3(s);
                for (int i = 0; i < countVerts; i++)
                    normals[i] = new MODVector3(s);
                countFaces = Helper.ReadU32BE(s);
                faces = new MODFace[countFaces];
                for (int i = 0; i < countFaces; i++)
                    faces[i] = new MODFace(s);
            }
            public string SaveToObj(int idx, ref int vcount)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("o Part_" + idx.ToString("D4"));
                foreach (MODVector3 v in vertices)
                    sb.AppendLine(v.ToOBJ("v"));
                foreach (MODVector3 n in normals)
                    sb.AppendLine(n.ToOBJ("vn"));
                sb.AppendLine("s off");
                foreach (MODFace f in faces)
                    sb.Append(f.ToOBJ(vcount));
                vcount += (int)countVerts;
                return sb.ToString();
            }
        }

        public class MODBBox
        {
            public ushort[] values = new ushort[4];
            public MODBBox(Stream s)
            {
                for (int i = 0; i < 4; i++)
                    values[i] = Helper.ReadU16BE(s);
            }
        }

        public class MODVector3
        {
            public float X, Y, Z, Padding;
            public MODVector3(Stream s)
            {
                X = ((short)Helper.ReadU16BE(s)) / 16f;
                Y = ((short)Helper.ReadU16BE(s)) / 16f;
                Z = ((short)Helper.ReadU16BE(s)) / 16f;
                Padding = (short)Helper.ReadU16BE(s);
            }

            public string ToOBJ(string type)
            {
                return type + " " + F(X) + " " + F(Y) + " " + F(Z);
            }

            private string F(float f)
            {
                return f.ToString().Replace(",", ".");
            }
        }

        public class MODFace
        {
            private byte[] raw;
            public ushort f1, f2, f3, f4;
            public byte flags;
            public MODFace(Stream s)
            {
                raw = new byte[0x54];
                s.Read(raw, 0, 0x54);
                MemoryStream m = new MemoryStream(raw);
                m.Seek(0x48, 0);
                f1 = Helper.ReadU16BE(m);
                f2 = Helper.ReadU16BE(m);
                f3 = Helper.ReadU16BE(m);
                f4 = Helper.ReadU16BE(m);
                f1++;
                f2++;
                f3++;
                f4++;
                flags = raw[0x53];
            }

            public string ToOBJ(int offset)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("f " + (f1 + offset) + "//" + (f1 + offset) + " "
                                   + (f2 + offset) + "//" + (f2 + offset) + " "
                                   + (f3 + offset) + "//" + (f3 + offset));
                if ((flags & 0x8) != 0)
                    sb.AppendLine("f " + (f2 + offset) + "//" + (f2 + offset) + " "
                                       + (f3 + offset) + "//" + (f3 + offset) + " "
                                       + (f4 + offset) + "//" + (f4 + offset));
                return sb.ToString();
            }
        }
    }
}
