using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrocExplorerWV
{
    public static class FileSystem
    {
        public static List<string> idxFilenames = new List<string>();
        public static List<IdxFile> idxFiles = new List<IdxFile>();
        public static string basePath;

        public static void Init(string path)
        {
            basePath = path;
            Log.WriteLine("Loading files from path : " + path);
            idxFilenames = new List<string>(SearchIdxFiles(basePath));
            idxFiles = new List<IdxFile>();
            foreach (string file in idxFilenames)
            {
                string check = Path.GetDirectoryName(file) + "\\" + Path.GetFileNameWithoutExtension(file) + ".wad";
                if (!File.Exists(check))
                    continue;
                IdxFile idx = new IdxFile(file, basePath.Length + 6);
                Log.WriteLine("Adding " + idx.filename + " ...");
                idxFiles.Add(idx);
            }
        }

        public static string[] SearchIdxFiles(string dir)
        {
            List<string> result = new List<string>();
            foreach (string d in Directory.GetDirectories(dir))
            {
                foreach (string f in Directory.GetFiles(d))
                    if (f.ToLower().EndsWith(".idx"))
                        result.Add(f);
                result.AddRange(SearchIdxFiles(d));
            }
            return result.ToArray();
        }
    }
}
