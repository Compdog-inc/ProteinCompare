using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public class CsvDialect
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly char[] PossibleDelimiters = new[] { '|', ',', ';', '\t' };
        public static readonly char?[] PossibleQuotes = new char?[] { '"', '\'', null };
        public static readonly char?[] PossiblesEscapes = new char?[] { '"', '\'', '\\', null };

        public char Delimiter { get; set; }
        public char? Quote { get; set; }
        public char? Escape { get; set; }

        public CsvDialect(char delimiter, char? quote, char? escape)
        {
            Delimiter = delimiter;
            Quote = quote;
            Escape = escape;
        }

        public static bool TryParseDelimiter(string[] sample, char delimiter, out int columnCount)
        {
            logger.Trace("Trying to parse delimiter {delimiter}", delimiter);
            columnCount = -1;

            for(int i=0;i<sample.Length; i++)
            {
                int tmp = sample[i].Split(delimiter).Length;
                if (i == 0)
                {
                    logger.Trace("First row has {column_count} column(s)", tmp);
                    columnCount = tmp;
                }
                else if (tmp != columnCount) // column count not consistent
                {
                    logger.Error("TryParseDelimiter failed: {current_count} != {target_count} @ {row}", tmp, columnCount, sample[i]);
                    if (i == 1)
                    {
                        logger.Warn("TryParseDelimiter failed on second row. Possible header, resetting columns");
                        columnCount = tmp;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool TryParseQuote(string[] sample, char delimiter, char? quote)
        {
            logger.Trace("Trying to parse quote {quote}", quote);
            if (quote == null) return true; // always a possibility
            for(int i=0;i<sample.Length;i++)
            {
                var columns = sample[i].Split(delimiter);
                bool bad = false;
                foreach (var column in columns)
                {
                    if (column.Length < 2)
                    {
                        logger.Trace("TryParseQuote failed: invalid column {column}", column);
                        bad = true;
                        break;
                    }

                    if (column[0] != quote)
                    {
                        logger.Trace("TryParseQuote failed: invalid first char {char}", column[0]);
                        bad = true;
                        break;
                    }

                    if (column[^1] != quote)
                    {
                        logger.Trace("TryParseQuote failed: invalid last char {char}", column[^1]);
                        bad = true;
                        break;
                    }
                }

                if (bad)
                {
                    if (i == 0)
                    {
                        logger.Warn("TryParseQuote failed on first row. Possible header, ignoring");
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            // all rows and columns passed
            return true;
        }

        public static bool TryParseEscape(string[] sample, char delimiter, char? quote, char? escape)
        {
            logger.Trace("Trying to parse escape {escape}", escape);
            if (escape == null) return true; // always a possibility
            foreach (var row in sample)
            {
                var columns = row.Split(delimiter);
                foreach (var column in columns)
                {
                    string content;
                    if (quote == null)
                        content = column;
                    else
                        content = column[1..^2];

                    if (content.Length > 0)
                    {
                        char definitelyNot = content[0]; // first char cannot be escaped
                        for (int i = 1; i < content.Length; i++)
                        {
                            if (
                                // escapable characters
                                content[i] == 't' ||
                                content[i] == 'b' ||
                                content[i] == 'n' ||
                                content[i] == 'r' ||
                                content[i] == 'f' ||
                                content[i] == 's' ||
                                content[i] == '\'' ||
                                content[i] == '"' ||
                                content[i] == '\\' ||
                                content[i] == quote // can't use quotes without escape
                                )
                            {
                                char possibleEscape = content[i - 1];
                                if (possibleEscape != definitelyNot && possibleEscape == escape)
                                    return true;
                            }
                        }
                    }
                }
            }

            // no escape found
            return false;
        }

        /// <summary>
        /// Tries to detect dialect from csv sample
        /// </summary>
        public static CsvDialect Detect(string[] sample, params char[] delimiters)
        {
            // delimiter detection
            var delims = delimiters.Concat(PossibleDelimiters).Distinct().ToArray();
            logger.Trace("Checking for [{delims}]", delims);

            var tries = delims.Select(d =>
            {
                bool possible = TryParseDelimiter(sample, d, out int columnCount);
                return (possible, columnCount);
            }).ToArray();

            char delimiter = ',';
            int columnCount = -1;
            for (int i = 0; i < tries.Length; i++)
            {
                if (tries[i].possible)
                {
                    // get delimiter with biggest column count
                    if (tries[i].columnCount > columnCount)
                    {
                        columnCount = tries[i].columnCount;
                        delimiter = delims[i];
                    }
                }
            }

            logger.Debug("Detected delimiter {delimiter} with {column_count} column(s)", delimiter, columnCount);

            // quote detection
            logger.Trace("Checking for [{quotes}]", PossibleQuotes);
            var quoteTries = PossibleQuotes.Select(q =>
            {
                bool possible = TryParseQuote(sample, delimiter, q);
                return possible;
            }).ToArray();

            char? quote = null;
            for (int i = 0; i < quoteTries.Length; i++)
            {
                if (quoteTries[i])
                {
                    quote = PossibleQuotes[i];
                    break; // order important
                }
            }

            logger.Debug("Detected quote {quote}", quote);

            // escape detection
            logger.Trace("Checking for [{escape}]", PossiblesEscapes);
            var escapeTries = PossiblesEscapes.Select(e =>
            {
                bool possible = TryParseEscape(sample, delimiter, quote, e);
                return possible;
            }).ToArray();

            char? escape = null;
            for (int i = 0; i < escapeTries.Length; i++)
            {
                if (escapeTries[i])
                {
                    escape = PossiblesEscapes[i];
                    break; // order important
                }
            }

            logger.Debug("Detected escape {escape}", escape);

            return new CsvDialect(delimiter, quote, escape);
        }
    }
}
