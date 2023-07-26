using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public static class ProteinIntersect
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static List<string> _intersectArrays(List<string> a, List<string> b, bool ignoreCase)
        {
            var result = new List<string>(Math.Min(a.Count, b.Count));
            int i = 0;
            int j = 0;
            while (i < a.Count && j < b.Count)
            {
                switch (string.Compare(a[i], b[j], ignoreCase))
                {
                    case 0:
                        result.Add(a[i]);
                        i++;
                        j++;
                        break;
                    case 1:
                        j++;
                        break;
                    case -1:
                        i++;
                        break;
                }
            }

            return result;
        }

        private static List<string>[] _reduceByIntersecting(List<string>[] array, bool ignoreCase)
        {
            if (array.Length <= 1) return array;

            List<string>[] output = new List<string>[array.Length / 2];
            logger.Trace("Reducing array: {input} -> {output}", array.Length, output.Length);

            for (int i = 0; i < output.Length; i++)
            {
                output[i] = _intersectArrays(array[i * 2 + 0], array[i * 2 + 1], ignoreCase);
            }

            if (array.Length % 2 != 0)
            {
                // odd reduction: intersect last even with last odd
                output[^1] = _intersectArrays(output[^1], array[^1], ignoreCase);
            }

            return output;
        }

        public static string[] Run(CsvTable[] tables, IEnumerable<string>? excluded, bool ignoreCase)
        {
            List<string>[] proteinLists = new List<string>[tables.Length];
            for(int i = 0; i < tables.Length; i++)
            {
                proteinLists[i] = new List<string>();

                logger.Trace("Generating protein list {current}/{total}", i + 1, tables.Length);
                foreach(var row in tables[i].Rows)
                {
                    if(row.AttachedData is string protein && !(excluded?.Contains(protein, new StringComparer(ignoreCase)) ?? false))
                    {
                        proteinLists[i].Add(protein);
                    }
                }
                logger.Trace("Sorting protein list");
                proteinLists[i].Sort(string.Compare);
            }

            while(proteinLists.Length > 1)
            {
                // always divides list by 2
                proteinLists = _reduceByIntersecting(proteinLists, ignoreCase);
            }

            return proteinLists[0].ToArray();
        }
    }
}
