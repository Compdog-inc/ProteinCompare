using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public static class FileFilter
    {
        public static bool FilterPaths(string path, char rowDelimiter)
        {
            if (!File.Exists(path)) return false;
            using FileStream fs = File.OpenRead(path);
            using StreamReader sr = new(fs);
            int c = 1024;
            while (c > 0 && !sr.EndOfStream)
            {
                int n = sr.Read();
                if (n >= 0)
                {
                    if ((char)n == rowDelimiter)
                        return true;
                }
                c--;
            }
            return false;
        }
    }
}
