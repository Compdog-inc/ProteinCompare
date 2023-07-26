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

        public static string[] TransformRow(string row, CsvDialect dialect)
        {
            var columns = row.Split(dialect.Delimiter);

            // remove quotes
            if (dialect.Quote != null)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    columns[i] = columns[i][1..^2];
                }
            }

            // remove escape
            if (dialect.Escape != null)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    StringBuilder sb = new();

                    for (int j = 0; j < columns[i].Length; j++)
                    {
                        if (columns[i][j] == dialect.Escape && j < columns[i].Length - 1)
                        {
                            // detect escaped char and skip escape sequence
                            switch (columns[i][j + 1])
                            {
                                case 't':
                                    sb.Append('\t');
                                    j++;
                                    break;
                                case 'b':
                                    sb.Append('\b');
                                    j++;
                                    break;
                                case 'n':
                                    sb.Append('\n');
                                    j++;
                                    break;
                                case 'r':
                                    sb.Append('\r');
                                    j++;
                                    break;
                                case 'f':
                                    sb.Append('\f');
                                    j++;
                                    break;
                                case 's':
                                    sb.Append(' ');
                                    j++;
                                    break;
                                case '\'':
                                    sb.Append('\'');
                                    j++;
                                    break;
                                case '"':
                                    sb.Append('"');
                                    j++;
                                    break;
                                case '\\':
                                    sb.Append('\\');
                                    j++;
                                    break;
                                default:
                                    if (columns[i][j + 1] == dialect.Quote && dialect.Quote != null)
                                    {
                                        // quote escaped
                                        sb.Append(dialect.Quote);
                                        j++;
                                    } else
                                    {
                                        logger.Trace("Unknown escape sequence {char}", columns[i][j + 1]);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            sb.Append(columns[i][j]);
                        }
                    }

                    columns[i] = sb.ToString();
                }
            }

            return columns;
        }
    }
}
