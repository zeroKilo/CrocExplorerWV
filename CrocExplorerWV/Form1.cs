using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;

namespace CrocExplorerWV
{
    public partial class Form1 : Form
    {
        PIXFile currentPix;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Log.box = rtb1;
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "croc.exe|croc.exe";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileSystem.Init(Path.GetDirectoryName(d.FileName) + "\\");
                this.TopMost = true;
                Application.DoEvents();
                this.TopMost = false;
                RefreshTree();
                RefreshList();
            }
            else
                this.Close();
        }

        public void RefreshTree()
        {
            tv1.Nodes.Clear();
            foreach (IdxFile idx in FileSystem.idxFiles)
            {
                TreeNode nt = idx.ToTree();
                bool found = false;
                foreach(TreeNode t in tv1.Nodes)
                    if (t.Text == nt.Text)
                    {
                        t.Nodes.Add(nt.Nodes[0]);
                        found = true;
                        break;
                    }
                if(!found)
                    tv1.Nodes.Add(nt);
            }
        }

        public void RefreshList()
        {
            string[] files = Directory.GetFiles(FileSystem.basePath, "*.pix", SearchOption.AllDirectories);
            List<string> fileList = new List<string>();
            foreach (string file in files)
                fileList.Add(file);
            foreach (IdxFile idx in FileSystem.idxFiles)
                foreach (FileReference r in idx.refs)
                    if (r.name.ToLower().EndsWith(".pix"))
                        fileList.Add(idx.basepath + idx.filename + ">" + r.name);
            listBox1.Items.Clear();
            string filter = textBox1.Text.ToLower();
            foreach (string s in fileList)
                if (s.ToLower().Contains(filter))
                    listBox1.Items.Add(s);
        }

        private void tv1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode sel = e.Node;
            IdxFile idx = null;
            FileReference fr = null;
            Helper.GetFileRefrence(sel, out idx, out fr);
            if (fr != null)
            {
                byte[] data = idx.LoadEntry(fr);
                hb1.ByteProvider = new DynamicByteProvider(data);
            }
        }

        private void exportRAWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.*|*.*";
            if (tv1.SelectedNode != null)
                d.FileName = tv1.SelectedNode.Text;
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MemoryStream m = new MemoryStream();
                for (int i = 0; i < hb1.ByteProvider.Length; i++)
                    m.WriteByte(hb1.ByteProvider.ReadByte(i));
                File.WriteAllBytes(d.FileName, m.ToArray());
                Log.WriteLine("Saved to " + d.FileName);
            }
        }

        private void importRAWToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IdxFile idx = null;
            FileReference fr = null;
            Helper.GetFileRefrence(tv1.SelectedNode, out idx, out fr);
            if (fr != null)
            {
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "*.*|*.*";
                if (tv1.SelectedNode != null)
                    d.FileName = tv1.SelectedNode.Text;
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    byte[] buff = File.ReadAllBytes(d.FileName);
                    uint ucsize = (uint)buff.Length;
                    if (fr.compression != 0)
                        buff = Helper.CompressRLE(buff, fr.compression);
                    idx.SaveEntry(fr, buff, ucsize);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {            
            try
            {
                int n = comboBox1.SelectedIndex;
                if (n == -1)
                    return;
                PIXFile.PIXHeader h = currentPix.headers[n];
                PIXFile.PIXData d = currentPix.images[n];
                pb1.Image = PIXFile.MakeBitmap(h, d);
            }
            catch { }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            try
            {
                string s = listBox1.SelectedItem.ToString();
                currentPix = new PIXFile(LoadPixFile(s));
                comboBox1.Items.Clear();
                int count = 1;
                foreach (PIXFile.PIXHeader h in currentPix.headers)
                    comboBox1.Items.Add(count++ + "/" + currentPix.headers.Count + " " + h.name);
                if (comboBox1.Items.Count != 0)
                    comboBox1.SelectedIndex = 0;
            }
            catch { }
        }

        private byte[] LoadPixFile(string s)
        {
            byte[] result = null;
            if (s.Contains(">"))
            {
                string[] parts = s.Split('>');
                IdxFile idf = null;
                foreach (IdxFile idx in FileSystem.idxFiles)
                    if (idx.basepath + idx.filename == parts[0])
                    {
                        idf = idx;
                        break;
                    }
                if (idf == null)
                    return result;
                FileReference r = null;
                foreach (FileReference fr in idf.refs)
                    if (fr.name == parts[1])
                    {
                        r = fr;
                        break;
                    }
                if (r == null)
                    return result;
                result = idf.LoadEntry(r);
            }
            else
                result = File.ReadAllBytes(s);
            return result;
        }

        private void exportBMPToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (pb1.Image == null)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.png|*.png";
            if (currentPix != null && comboBox1.SelectedIndex != -1 && currentPix.headers.Count > 0 && currentPix.images.Count > 0)
            {
                d.FileName = currentPix.headers[comboBox1.SelectedIndex].name;
                if (!d.FileName.ToLower().EndsWith(".png"))
                    d.FileName += ".png";
            }
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pb1.Image.Save(d.FileName);
                Log.WriteLine("Saved to " + d.FileName);
            }
        }

        private void importPNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1 && comboBox1.SelectedIndex != -1)
            {
                string s = listBox1.SelectedItem.ToString();
                byte[] pxData = LoadPixFile(s);
                PIXFile px = new PIXFile(pxData);
                if (px == null)
                    return;
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "*.png|*.png";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Bitmap bmp = new Bitmap(d.FileName);
                    PIXFile.PIXHeader h = px.headers[comboBox1.SelectedIndex];
                    if (bmp.Width != h.Width || bmp.Height != h.Height)
                    {
                        MessageBox.Show("Imported image size doesnt match! (" + bmp.Width + "x" + bmp.Height + " vs " + h.Width + "x" + h.Height + ")");
                        return;
                    }
                    byte[] data = PIXFile.MakePixdata(h, bmp);
                    int start = (int)px.images[comboBox1.SelectedIndex]._fileOffset + 12;
                    for (int i = 0; i < data.Length; i++)
                        pxData[start + i] = data[i];
                    if (s.Contains(">"))
                    {
                        string[] parts = s.Split('>');
                        IdxFile idf = null;
                        foreach (IdxFile idx in FileSystem.idxFiles)
                            if (idx.basepath + idx.filename == parts[0])
                            {
                                idf = idx;
                                break;
                            }
                        if (idf == null)
                        {
                            Log.WriteLine("Error: IDX File not found (" + parts[0] + ")");
                            return;
                        }
                        FileReference r = null;
                        foreach (FileReference fr in idf.refs)
                            if (fr.name == parts[1])
                            {
                                r = fr;
                                break;
                            }
                        if (r == null)
                        {
                            Log.WriteLine("Error: File reference not found (" + parts[1] + ")");
                            return;
                        }
                        uint ucsize = (uint)pxData.Length;
                        if (r.compression != 0)
                            pxData = Helper.CompressRLE(pxData, r.compression);
                        idf.SaveEntry(r, pxData, ucsize);
                    }
                    else
                        File.WriteAllBytes(s, pxData);
                    Log.WriteLine("Saved to " + s);
                }
            }
        }

        private void exportAllPIXAsBMPToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog d = new FolderBrowserDialog();
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string output = d.SelectedPath + "\\";
                int count = 0;
                prog.Minimum = 0;
                prog.Maximum = listBox1.Items.Count;
                foreach (string entry in listBox1.Items)
                {
                    if ((count % 10) == 0)
                    {
                        prog.Value = count;
                        Application.DoEvents();                        
                    }
                    if (entry.Contains(">"))
                        ExportPNGfromPIX(LoadPixFile(entry), Path.GetFileName(entry.Split('>')[1]), output + Path.GetFileName(entry.Split('>')[0]) + "\\");
                    else
                        ExportPNGfromPIX(File.ReadAllBytes(entry), Path.GetFileName(entry), output);
                    count++;
                }
                prog.Value = 0;
                Log.WriteLine("Done.");
            }
        }

        private void ExportPNGFromWAD(string entry, string output)
        {
            string[] parts = entry.Split('>');
        }

        private void ExportPNGfromPIX(byte[] data, string pixname, string output)
        {
            if (data == null)
                return;
            PIXFile px = new PIXFile(data);
            string dir = output;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            if (px.headers.Count > 1)
            {
                dir += pixname;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                dir += "\\";
            }
            for (int i = 0; i < px.headers.Count; i++)
            {
                PIXFile.PIXHeader h = px.headers[i];
                PIXFile.PIXData d = px.images[i];
                Bitmap bmp = PIXFile.MakeBitmap(h, d);
                string name = MakeName(dir + Sanitize(h.name));
                bmp.Save(name);
                Log.WriteLine("Saved " + name);
            }
        }

        private string MakeName(string name)
        {
            string result = name;
            if (File.Exists(result + ".png"))
            {
                int count = 0;
                while (File.Exists(result + "_" + count + ".png"))
                    count++;
                result += "_" + count + ".png";
            }
            else
                result += ".png";
            return result;
        }

        private string Sanitize(string s)
        {
            return s.Replace("?", "_QUESTIONMARK_")
                    .Replace(":", "_COLON_")
                    .Replace(";", "_SEMICOLON_")
                    .Replace(".", "_DOT_")
                    .Replace(",", "_COMMA_")
                    .Replace("<", "_LESS_")
                    .Replace(">", "_GREATER_")
                    .Replace("[", "_BRACKETOPEN_")
                    .Replace("]", "_BRACKETCLOSED_")
                    .Replace("%", "_PERCENT_")
                    .Replace("/", "_SLASH_")
                    .Replace("\\", "_BACKSLASH_")
                    .Replace("'", "_QUOTE_")
                    .Replace("\"", "_DBLQUOTE_")
                    .Replace("*", "_STAR_");
        }
    }
}
