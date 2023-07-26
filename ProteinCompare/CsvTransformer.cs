using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public static class CsvTransformer
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static string[] TransformText(string text, char rowDelimiter)
        {
            return text.Split(rowDelimiter);
        }

        public static bool TryEscape(string sequence, CsvDialect dialect, out char escapedChar)
        {
            escapedChar = '\0';
            if (dialect.Escape == null || string.IsNullOrEmpty(sequence) || sequence[0] != dialect.Escape)
                return false;

            if (sequence.Length > 1)
            {
                // detect escaped char and skip escape sequence
                switch (sequence[1])
                {
                    case 't':
                        escapedChar = '\t';
                        return true;
                    case 'b':
                        escapedChar = '\b';
                        return true;
                    case 'n':
                        escapedChar = '\n';
                        return true;
                    case 'r':
                        escapedChar = '\r';
                        return true;
                    case 'f':
                        escapedChar = '\f';
                        return true;
                    case 's':
                        escapedChar = ' ';
                        return true;
                    case '\'':
                        escapedChar = '\'';
                        return true;
                    case '"':
                        escapedChar = '"';
                        return true;
                    case '\\':
                        escapedChar = '\\';
                        return true;
                    default:
                        if (dialect.Quote != null && sequence[1] == dialect.Quote)
                        {
                            // quote escaped
                            escapedChar = dialect.Quote ?? '\0';
                            return true;
                        }
                        return false;
                }
            }

            return false;
        }

        public static string[] TransformRow(string row, CsvDialect dialect)
        {
            bool inColumn = false;
            List<string> columns = new();
            StringBuilder currentColumn = new();

            for (int i = 0; i < row.Length; i++)
            {
                // only escape if inside column contents
                if ((inColumn || dialect.Quote == null) && TryEscape(row[i..], dialect, out char escapedChar))
                {
                    currentColumn.Append(escapedChar);
                    i++;
                }
                else if (dialect.Quote != null && row[i] == dialect.Quote) // detect quotes
                {
                    inColumn = !inColumn;
                }
                else if ((!inColumn || dialect.Quote == null) && row[i] == dialect.Delimiter)
                {
                    columns.Add(currentColumn.ToString());
                    currentColumn.Clear();
                }
                else
                {
                    currentColumn.Append(row[i]);
                }
            }

            if(currentColumn.Length > 0)
            {
                columns.Add(currentColumn.ToString());
            }

            return columns.ToArray();
        }
    }
}
