using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrocExplorerWV
{
    public static class Log
    {
        public static RichTextBox box = null;
        public static readonly object _sync = new object();

        public static void WriteLine(string s)
        {
            Write(s + "\n");
        }

        public static void Write(string s)
        {
            if (box == null)
                return;
            box.BeginInvoke((MethodInvoker)delegate()
            {
                box.AppendText(s);
                box.SelectionStart = box.Text.Length;
                box.ScrollToCaret();
            });
        }
    }
}
