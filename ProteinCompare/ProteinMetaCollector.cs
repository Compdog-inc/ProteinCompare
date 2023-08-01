using NLog;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public static class ProteinMetaCollector
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static CsvTable[] ReduceByProtein(CsvTable[] tables, IEnumerable<string>? excluded, bool ignoreCase, string[]? proteins, IEnumerable<string>? filter)
        {
            CsvTable[] result = new CsvTable[tables.Length];

            using (var pbar = new ProgressBar(tables.Select(t => t.Rows.Length).Aggregate((a, b) => a + b), "Reducing proteins", new ProgressBarOptions()
            {
                ProgressBarOnBottom = true
            }))
            {
                for (int i = 0; i < tables.Length; i++)
                {
                    logger.Trace("Reducing table {current}/{total}", (i + 1), tables.Length);
                    var list = new Dictionary<string, CsvEntry>();
                    foreach (var row in tables[i].Rows)
                    {
                        if (
                            row.AttachedData is string protein &&
                            !(excluded?.Contains(protein, new StringComparer(ignoreCase)) ?? false) &&
                            (proteins == null || proteins.Contains(protein, new StringComparer(ignoreCase)))
                            )
                        {
                            var filteredRow = new CsvRow(row.Index, filter == null ? row.Values : row.Values.Where((v) => filter.Contains(tables[i].Columns[v.ColumnIndex].Name)).ToArray());
                            var filteredColumns = filter == null ? tables[i].Columns : tables[i].Columns.Where((v)=>filter.Contains(v.Name)).ToArray();
                            if (list.ContainsKey(ignoreCase ? protein.ToUpperInvariant() : protein))
                            {
                                var merged = list[ignoreCase ? protein.ToUpperInvariant() : protein].Merge(new CsvEntry(filteredRow, filteredColumns)); // merge current entry with existing
                                list[ignoreCase ? protein.ToUpperInvariant() : protein] = merged;
                            }
                            else
                            {
                                list.Add(ignoreCase ? protein.ToUpperInvariant() : protein, new CsvEntry(filteredRow, filteredColumns));
                            }
                        }
                        pbar.Tick("Reducing proteins: table " + (i + 1) + "/" + tables.Length);
                    }

                    var header = new List<CsvColumn>();
                    foreach(var pair in list)
                    {
                        foreach(var col in pair.Value.Columns)
                        {
                            if (!header.Contains(col))
                            {
                                header.Add(col);
                            }
                        }
                    }

                    var csv = new CsvBuilder()
                        .AddColumn("Protein", CsvType.String)
                        .AddColumns(header.ToArray());
                    foreach (var pair in list)
                    {
                        csv.AddToRow(CsvExtensions.SerializeObject(pair.Key));
                        csv.AddToRow(pair.Value.Row.Values);
                        csv.PushRow();
                    }
                    result[i] = csv.ToTable();
                }
            }

            return result;
        }

        public static CsvTable[] ReduceByProteinMerged(CsvTable[] tables, IEnumerable<string>? excluded, bool ignoreCase, string[]? proteins, IEnumerable<string>? filter)
        {
            var list = new Dictionary<string, CsvEntry>();

            using (var pbar = new ProgressBar(tables.Select(t => t.Rows.Length).Aggregate((a, b) => a + b), "Reducing proteins", new ProgressBarOptions()
            {
                ProgressBarOnBottom = true
            }))
            {
                for (int i = 0; i < tables.Length; i++)
                {
                    logger.Trace("Reducing table {current}/{total}", (i + 1), tables.Length);
                    foreach (var row in tables[i].Rows)
                    {
                        if (
                            row.AttachedData is string protein &&
                            !(excluded?.Contains(protein, new StringComparer(ignoreCase)) ?? false) &&
                            (proteins == null || proteins.Contains(protein, new StringComparer(ignoreCase)))
                            )
                        {
                            var filteredRow = new CsvRow(row.Index, filter == null ? row.Values : row.Values.Where((v) => filter.Contains(tables[i].Columns[v.ColumnIndex].Name)).ToArray());
                            var filteredColumns = filter == null ? tables[i].Columns : tables[i].Columns.Where((v) => filter.Contains(v.Name)).ToArray();

                            if (list.ContainsKey(ignoreCase ? protein.ToUpperInvariant() : protein))
                            {
                                var merged = list[ignoreCase ? protein.ToUpperInvariant() : protein].Merge(new CsvEntry(filteredRow, filteredColumns)); // merge current entry with existing
                                list[ignoreCase ? protein.ToUpperInvariant() : protein] = merged;
                            }
                            else
                            {
                                list.Add(ignoreCase ? protein.ToUpperInvariant() : protein, new CsvEntry(filteredRow, filteredColumns));
                            }
                        }
                        pbar.Tick("Reducing proteins: table " + (i + 1) + "/" + tables.Length);
                    }
                }
            }

            var header = new List<CsvColumn>();
            foreach (var pair in list)
            {
                foreach (var col in pair.Value.Columns)
                {
                    if (!header.Contains(col))
                    {
                        header.Add(col);
                    }
                }
            }

            var csv = new CsvBuilder()
                .AddColumn("Protein", CsvType.String)
                .AddColumns(header.ToArray());
            foreach (var pair in list)
            {
                csv.AddToRow(CsvExtensions.SerializeObject(pair.Key));
                csv.AddToRow(pair.Value.Row.Values);
                csv.PushRow();
            }

            return new CsvTable[1] { csv.ToTable() };
        }
    }
}
