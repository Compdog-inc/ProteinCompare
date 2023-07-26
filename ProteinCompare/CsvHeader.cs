using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public static class CsvHeader
    {
        public static CsvColumn[] DetectHeader(string[] sample, CsvDialect dialect)
        {
            var firstRow = CsvTransformer.TransformRow(sample[0], dialect);
            var columnTypes = CsvParser.TryDetectColumnTypes(sample, dialect);

            if(firstRow.Length > columnTypes.Length)
            {
                firstRow = firstRow.SkipLast(firstRow.Length - columnTypes.Length).ToArray();
            } else if(firstRow.Length < columnTypes.Length)
            {
                firstRow = firstRow.Concat(Enumerable.Repeat("", columnTypes.Length - firstRow.Length)).ToArray();
            }

            // see if first row has a different type than other rows
            bool possiblyHeader = true;
            for (int i = 0; i < firstRow.Length; i++)
            {
                var type = CsvParser.DetectContentType(firstRow[i]);
                if (
                    type != CsvType.String || // header always has column names
                    (type == columnTypes[i] && columnTypes[i] != CsvType.String) // if both are string there is no way to tell
                ){
                    possiblyHeader = false;
                    break;
                }
            }

            if (possiblyHeader)
            {
                return columnTypes.Select((t, i) => new CsvColumn(firstRow[i], t)).ToArray();
            }
            else
            {
                return columnTypes.Select(t => new CsvColumn(t)).ToArray();
            }
        }
    }
}
