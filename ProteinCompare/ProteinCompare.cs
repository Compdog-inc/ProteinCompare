﻿using CommandLine;
using NLog;
using NLog.Layouts;
using NLog.Targets;

namespace ProteinCompare
{
    class ProteinCompare
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Enable verbose messages.", Default = false)]
            public bool Verbose { get; set; }

            [Option('d', "delimiters", Required = false, MetaValue = "<char>", HelpText = "Add custom CSV column delimiters used for detection.")]
            public IEnumerable<char>? Delimiters { get; set; }

            [Option('r', "rowdelim", Required = false, MetaValue = "<char>", HelpText = "(Default: \\n) Custom row delimiter for parsing.")]
            public char? RowDelimiter { get; set; }

            [Option('s', "sample", Required = false, Default = 256, MetaValue = "<int:(row count)>", HelpText = "Set CSV table sample size in rows for detection.")]
            public int SampleSize { get; set; }

            [Option('h', "safe", Required = false, Default = 2, MetaValue = "<int:(row count)>", HelpText = "Set CSV table safe row count (allows detection errors).\nSet to the minimum header/abnormal row count.")]
            public int SafeRowCount { get; set; }

            [Value(0, MetaName = "[...files]", MetaValue = "<string:(path)>", HelpText = "List of paths to protein files (separated by spaces and supports wildcards)")]
            public IEnumerable<string>? Files { get; set; }
        }

        public static int Main(string[] args)
        {
            return new Parser(p =>
            {
                p.EnableDashDash = true;
                p.AutoHelp = true;
                p.HelpWriter = Console.Error;
            }).ParseArguments<Options>(args)
            .MapResult(
              options => new ProteinCompare(options).Start(),
              _ => 1);
        }

        private Options options;

        public ProteinCompare(Options options)
        {
            this.options = options;
            LogManager.Setup().LoadConfiguration(builder =>
            {
                // StdOut for <= Warn
                builder.ForLogger().FilterLevels(options.Verbose ? LogLevel.Trace : LogLevel.Info, LogLevel.Warn).WriteTo(new ColoredConsoleTarget()
                {
                    StdErr = false,
                    AutoFlush = true,
                    Layout = Layout.FromMethod(evt =>
                {
                    return string.Format("{0}|{1}: {2}", evt.Level.Name.ToUpperInvariant(), evt.LoggerName, evt.FormattedMessage);
                })
                });

                // StdErr for >= Error
                builder.ForLogger().FilterMinLevel(LogLevel.Error).WriteTo(new ColoredConsoleTarget()
                {
                    StdErr = true,
                    AutoFlush = true,
                    Layout = Layout.FromMethod(evt =>
                    {
                        return string.Format("{0}|{1}: {2}", evt.Level.Name.ToUpperInvariant(), evt.LoggerName, evt.FormattedMessage);
                    })
                });
            });
        }

        public int Start()
        {
            if (options.Files == null)
            {
                logger.Warn("Files is null - treating same as empty.");
                return 0;
            }

            var files = FileParser.Parse(options.Files).ToArray();
            logger.Debug("Using protein files: [{files}]", files);

            if (files.Length == 0)
            {
                return 0;
            }

            logger.Trace("Filtering protein files");
            files = files.Where(p => FileFilter.FilterPaths(p, options.RowDelimiter ?? '\n')).ToArray();
            logger.Trace("Found {valid_files} valid file(s)", files.Length);

            List<CsvTable> tables = new(files.Length);
            foreach (var file in files)
            {
                try
                {
                    tables.Add(CsvReader.ReadFile(file, options.RowDelimiter ?? '\n', options.SampleSize, options.SafeRowCount, options.Delimiters?.ToArray() ?? Array.Empty<char>()));
                    logger.Trace("Loaded table {path}", file);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error loading file {file}", file);
                }
            }

            logger.Info("{file_count} file(s) loaded.", tables.Count);

            for (int i = 0; i < tables.Count; i++)
            {
                logger.Trace("Converting proteins {progress}/{total}", i + 1, tables.Count);
                foreach (var row in tables[i].Rows)
                {
                    if (row.Values.Length > 0)
                    {
                        if (ProteinDetector.GetProtein(row.Values[0], out string protein))
                        {
                            row.AttachedData = protein;
                        }
                    }
                }
            }

            logger.Info("Attached detected protein data.");

            return 0;
        }
    }
}