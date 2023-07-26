using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public static class FileParser
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static IEnumerable<string> _parseSegment(string path, string[] parts)
        {
            logger.Trace("Segment {path} / [{parts}]", path, parts);
            if (parts.Length == 0) return new[] { path };

            if (!parts[0].Contains('*'))
            {
                logger.Trace("Fully defined part");
                var basePath = Path.GetFullPath(parts[0] + '/', path).Replace('\\', '/');
                return _parseSegment(basePath, parts[1..]);
            }
            else if (!parts[0].Contains('.'))
            {
                logger.Trace("Partially definined directory");
                IEnumerable<string> paths = new List<string>();
                // relative base
                foreach (var basePath in Directory.GetDirectories(path, parts[0]))
                {
                    paths = paths.Concat(_parseSegment(basePath.Replace('\\', '/'), parts[1..]));
                }
                return paths;
            } else if (!string.IsNullOrEmpty(parts[0]))
            {
                logger.Trace("Partially definined file");
                IEnumerable<string> paths = new List<string>();
                // relative base
                foreach (var basePath in Directory.GetFiles(path, parts[0], SearchOption.TopDirectoryOnly))
                {
                    paths = paths.Concat(_parseSegment(basePath.Replace('\\', '/'), parts[1..]));
                }
                return paths;
            } else
            {
                logger.Trace("Unknown directory tree");
                IEnumerable<string> paths = new List<string>();

                // peek next path
                if (parts.Length > 1)
                {

                }

                // relative base
                foreach (var basePath in Directory.GetDirectories(path))
                {
                    // keep searching
                    paths = paths.Concat(_parseSegment(basePath.Replace('\\', '/'), parts));
                }
                return paths;
            }
        }

        public static IEnumerable<string> Parse(string path)
        {
            logger.Trace("Parsing {path}", path);
            string[] parts = path.Replace('\\', '/').Split('/');
            return _parseSegment("", parts);
        }

        public static IEnumerable<string> Parse(IEnumerable<string> paths)
        {
            return paths.SelectMany(Parse);
        }
    }
}
