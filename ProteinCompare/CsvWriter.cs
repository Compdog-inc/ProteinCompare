using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public static class CsvWriter
    {
        public static void WriteToTextWriter(TextWriter writer, CsvTable table, CsvDialect dialect, char rowDelimiter)
        {
            foreach (var row in table.Rows)
            {
                writer.Write(CsvTransformer.FormatRow(row.Values.Select(v => v.Value).ToArray(), dialect));
                writer.Write(rowDelimiter);
            }
        }

        public static void WriteToFile(string path, CsvTable table, CsvDialect dialect, char rowDelimiter)
        {
            using var fs = File.Create(path);
            using var writer = new StreamWriter(fs);
            WriteToTextWriter(writer, table, dialect, rowDelimiter);
        }

        public static string WriteToText(CsvTable table, CsvDialect dialect, char rowDelimiter)
        {
            using var writer = new StringWriter();
            WriteToTextWriter(writer, table, dialect, rowDelimiter);
            return writer.ToString();
        }
    }
}
