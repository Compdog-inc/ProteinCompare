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

        private static IEnumerable<string> _parseSegment(string path, string[] parts, int depth)
        {
            logger.Trace("Segment {path} | [{parts}] | {depth}", path, parts, depth);
            if (parts.Length == 0) return new[] { path };

            if (string.IsNullOrEmpty(parts[0]) && depth > 0)
            {
                logger.Trace("Unknown directory tree");
                IEnumerable<string> paths = new List<string>();

                // relative base
                foreach (var basePath in Directory.GetDirectories(path))
                {
                    // peek next path
                    if (parts.Length > 1)
                    {
                        logger.Trace("Peeking into {peek}", parts[1]);
                        paths = paths.Concat(_parseSegment(basePath.Replace('\\', '/'), parts[1..], depth+1));
                    }

                    // keep searching
                    paths = paths.Concat(_parseSegment(basePath.Replace('\\', '/'), parts, depth));
                }
                return paths;
            }
            else if (!parts[0].Contains('*'))
            {
                logger.Trace("Fully defined part");
                var basePath = Path.GetFullPath(parts[0] + '/', path).Replace('\\', '/');
                if(Path.Exists(basePath))
                    return _parseSegment(basePath, parts[1..], depth+1);
                else
                    return Array.Empty<string>();
            }
            else if (!parts[0].Contains('.'))
            {
                logger.Trace("Partially defined directory");
                IEnumerable<string> paths = new List<string>();
                // relative base
                foreach (var basePath in Directory.GetDirectories(path, parts[0]))
                {
                    paths = paths.Concat(_parseSegment(basePath.Replace('\\', '/'), parts[1..], depth + 1));
                }
                return paths;
            } else
            {
                logger.Trace("Partially defined file");
                IEnumerable<string> paths = new List<string>();
                // relative base
                foreach (var basePath in Directory.GetFiles(path, parts[0], SearchOption.TopDirectoryOnly))
                {
                    paths = paths.Concat(_parseSegment(basePath.Replace('\\', '/'), parts[1..], depth + 1));
                }
                return paths;
            }
        }

        public static IEnumerable<string> Parse(string path)
        {
            logger.Trace("Parsing {path}", path);
            string[] parts = path.Replace('\\', '/').Split('/');
            return _parseSegment(Directory.GetCurrentDirectory(), parts, 0);
        }

        public static IEnumerable<string> Parse(IEnumerable<string> paths)
        {
            return paths.SelectMany(Parse);
        }
    }
}
