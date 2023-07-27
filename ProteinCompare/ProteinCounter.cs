using NLog;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public static class ProteinCounter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static Dictionary<string, uint>[] Run(CsvTable[] tables, IEnumerable<string>? excluded, bool ignoreCase)
        {
            Dictionary<string, uint>[] lists = new Dictionary<string, uint>[tables.Length];

            using (var pbar = new ProgressBar(tables.Select(t => t.Rows.Length).Aggregate((a, b) => a + b), "Counting proteins", new ProgressBarOptions()
            {
                ProgressBarOnBottom = true
            }))
            {
                for (int i = 0; i < tables.Length; i++)
                {
                    logger.Trace("Counting table {current}/{total}", (i + 1), tables.Length);
                    var list = new Dictionary<string, uint>();
                    foreach (var row in tables[i].Rows)
                    {
                        if (row.AttachedData is string protein && !(excluded?.Contains(protein, new StringComparer(ignoreCase)) ?? false))
                        {
                            if (list.ContainsKey(ignoreCase ? protein.ToUpperInvariant() : protein))
                                list[ignoreCase ? protein.ToUpperInvariant() : protein]++;
                            else
                                list.Add(ignoreCase ? protein.ToUpperInvariant() : protein, 1);
                        }
                        pbar.Tick("Counting proteins: table " + (i + 1) + "/" + tables.Length);
                    }
                    lists[i] = list;
                }
            }

            return lists;
        }

        public static Dictionary<string, uint>[] RunList(CsvTable[] tables, IEnumerable<string>? excluded, bool ignoreCase, string[] proteins)
        {
            Dictionary<string, uint>[] lists = new Dictionary<string, uint>[tables.Length];

            using (var pbar = new ProgressBar(tables.Select(t => t.Rows.Length).Aggregate((a, b) => a + b), "Counting proteins", new ProgressBarOptions()
            {
                ProgressBarOnBottom = true
            }))
            {
                for (int i = 0; i < tables.Length; i++)
                {
                    logger.Trace("Counting table {current}/{total}", (i + 1), tables.Length);
                    var list = new Dictionary<string, uint>();
                    foreach (var row in tables[i].Rows)
                    {
                        if (row.AttachedData is string protein && !(excluded?.Contains(protein, new StringComparer(ignoreCase)) ?? false) && proteins.Contains(protein, new StringComparer(ignoreCase)))
                        {
                            if (list.ContainsKey(ignoreCase ? protein.ToUpperInvariant() : protein))
                                list[ignoreCase ? protein.ToUpperInvariant() : protein]++;
                            else
                                list.Add(ignoreCase ? protein.ToUpperInvariant() : protein, 1);
                        }
                        pbar.Tick("Counting proteins: table " + (i + 1) + "/" + tables.Length);
                    }
                    lists[i] = list;
                }
            }

            return lists;
        }

        public static Dictionary<string, uint>[] RunMerged(CsvTable[] tables, IEnumerable<string>? excluded, bool ignoreCase)
        {
            Dictionary<string, uint> list = new();

            using (var pbar = new ProgressBar(tables.Select(t => t.Rows.Length).Aggregate((a, b) => a + b), "Counting proteins", new ProgressBarOptions()
            {
                ProgressBarOnBottom = true
            }))
            {
                for (int i = 0; i < tables.Length; i++)
                {
                    logger.Trace("Counting table {current}/{total}", (i + 1), tables.Length);
                    foreach (var row in tables[i].Rows)
                    {
                        if (row.AttachedData is string protein && !(excluded?.Contains(protein, new StringComparer(ignoreCase)) ?? false))
                        {
                            if (list.ContainsKey(ignoreCase ? protein.ToUpperInvariant() : protein))
                                list[ignoreCase ? protein.ToUpperInvariant() : protein]++;
                            else
                                list.Add(ignoreCase ? protein.ToUpperInvariant() : protein, 1);
                        }
                        pbar.Tick("Counting proteins: table " + (i + 1) + "/" + tables.Length);
                    }
                }
            }

            return new[] { list };
        }

        public static Dictionary<string, uint>[] RunMergedList(CsvTable[] tables, IEnumerable<string>? excluded, bool ignoreCase, string[] proteins)
        {
            Dictionary<string, uint> list = new();

            using (var pbar = new ProgressBar(tables.Select(t => t.Rows.Length).Aggregate((a, b) => a + b), "Counting proteins", new ProgressBarOptions()
            {
                ProgressBarOnBottom = true
            }))
            {
                for (int i = 0; i < tables.Length; i++)
                {
                    logger.Trace("Counting table {current}/{total}", (i + 1), tables.Length);
                    foreach (var row in tables[i].Rows)
                    {
                        if (row.AttachedData is string protein && !(excluded?.Contains(protein, new StringComparer(ignoreCase)) ?? false) && proteins.Contains(protein, new StringComparer(ignoreCase)))
                        {
                            if (list.ContainsKey(ignoreCase ? protein.ToUpperInvariant() : protein))
                                list[ignoreCase ? protein.ToUpperInvariant() : protein]++;
                            else
                                list.Add(ignoreCase ? protein.ToUpperInvariant() : protein, 1);
                        }
                        pbar.Tick("Counting proteins: table " + (i + 1) + "/" + tables.Length);
                    }
                }
            }

            return new[] { list };
        }
    }
}
