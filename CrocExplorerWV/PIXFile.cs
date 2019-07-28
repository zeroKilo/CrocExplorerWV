using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrocExplorerWV
{
    public class PIXFile
    {
        public List<PIXHeader> headers = new List<PIXHeader>();
        public List<PIXData> images = new List<PIXData>();

        public PIXFile(byte[] data)
        {
            MemoryStream m = new MemoryStream(data);
            m.Seek(0x10, 0);
            try
            {
                while (m.Position < data.Length)
                {
                    ReadChunk(m);
                }
            }
            catch { }
        }

        public static byte[] MakePixdata(PIXHeader header, Bitmap bmp)
        {
            MemoryStream m = new MemoryStream();
            for (int y = 0; y < header.Height; y++)
                for (int x = 0; x < header.Width; x++)
                {
                    switch (header.PixelFormat)
                    {
                        case 5:
                            Color c = bmp.GetPixel(x, y);
                            ushort value = (ushort)(c.R >> 3);
                            value <<= 6;
                            value |= (ushort)(c.G >> 2);
                            value <<= 5;
                            value |= (ushort)(c.B >> 3);
                            m.WriteByte((byte)(value >> 8));
                            m.WriteByte((byte)value);
                            break;
                    }
                }
            return m.ToArray();
        }

        public static Bitmap MakeBitmap(PIXHeader header, PIXData image)
        {
            if (header == null || image == null)
                return null;
            Bitmap result = new Bitmap(header.Width, header.Height);
            Graphics gr = Graphics.FromImage(result);
            gr.Clear(Color.White);
            MemoryStream m = new MemoryStream(image.ImageData);
            for (int y = 0; y < header.Height; y++)
                for (int x = 0; x < header.Width; x++)
                {
                    switch (header.PixelFormat)
                    {
                        case 5:
                            m.Seek((x + y * header.Width) * image.BytesPerPixel, 0);
                            ushort value = Helper.ReadU16(m);
                            byte b = (byte)(value & 0x1F);
                            value >>= 5;
                            byte g = (byte)(value & 0x3F);
                            value >>= 6;
                            byte r = (byte)(value & 0x1F);
                            r <<= 3;
                            g <<= 2;
                            b <<= 3;
                            result.SetPixel(x, y, Color.FromArgb(255, r, g, b));
                            break;
                    }
                }
            return result;
        }

        private void ReadChunk(Stream s)
        {
            uint id = Helper.ReadU32(s);
            switch (id)
            {
                case 0x3D:
                    headers.Add(new PIXHeader(s));                    
                    break;
                case 0x21:
                    images.Add(new PIXData(s));
                    break;
                default:
                    Log.WriteLine("Found unknown chunk id=0x" + id.ToString("X"));
                    throw new Exception();
            }
        }

        public class PIXHeader
        {
            public uint _fileOffset;
            public uint Length;
            public byte PixelFormat;
            public ushort PageWidth;
            public ushort Width;
            public ushort Height;
            public ushort OffsetX;
            public ushort OffsetY;
            public ushort Unknown;
            public string name;

            public PIXHeader(Stream s)
            {
                _fileOffset = (uint)s.Position;
                Length = Helper.ReadU32(s);
                PixelFormat = (byte)s.ReadByte();
                PageWidth = Helper.ReadU16(s);
                Width = Helper.ReadU16(s);
                Height = Helper.ReadU16(s);
                OffsetX = Helper.ReadU16(s);
                OffsetY = Helper.ReadU16(s);
                Unknown = Helper.ReadU16(s);
                byte b;
                name = "";
                while ((b = (byte)s.ReadByte()) != 0)
                    name += (char)b;
            }
        }

        public class PIXData
        {
            public uint _fileOffset;
            public uint Length;
            public uint PixelCount;
            public uint BytesPerPixel;
            public byte[] ImageData;

            public PIXData(Stream s)
            {
                _fileOffset = (uint)s.Position;
                Length = Helper.ReadU32(s);
                PixelCount = Helper.ReadU32(s);
                BytesPerPixel = Helper.ReadU32(s);
                ImageData = new byte[Length];
                s.Read(ImageData, 0, (int)Length);
            }
        }
    }
}
