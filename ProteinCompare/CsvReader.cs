using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public static class CsvReader
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static CsvTable ReadText(string text, char rowDelimiter, int sampleSize, char[] columnDelimiters)
        {
            var raw_rows = CsvTransformer.TransformText(text, rowDelimiter);
            var sample = raw_rows[..Math.Min(raw_rows.Length - 1, sampleSize - 1)];
            logger.Trace("Detecting CSV with {row_length} row(s) using {sample_size} sample size.", raw_rows.Length, sample.Length);

            CsvDialect dialect = CsvDialect.Detect(sample, columnDelimiters);
            CsvColumn[] columns = CsvHeader.DetectHeader(sample, dialect);
            CsvRow[] rows = new CsvRow[raw_rows.Length];

            for (int i = 0; i < rows.Length; i++)
            {
                var values = CsvTransformer.TransformRow(raw_rows[i], dialect);
                CsvValue[] csvValues = new CsvValue[values.Length];

                for (int j = 0; j < csvValues.Length; j++)
                {
                    csvValues[j] = new CsvValue(values[j], j, columns[j].Type);
                }

                rows[i] = new CsvRow(i, csvValues);
            }

            return new CsvTable(columns, rows);
        }
    }
}
