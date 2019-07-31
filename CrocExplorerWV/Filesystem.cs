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

        public static void FindFile(string s, out IdxFile idf, out FileReference r)
        {
            r = null;
            idf = null;
            string[] parts = s.Split('>');
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
        }
    }
}
