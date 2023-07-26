using CommandLine;
using NLog;
using NLog.Layouts;
using NLog.Targets;

namespace ProteinCompare
{
    class ProteinCompare
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public interface GlobalOptions
        {
            [Option('v', "verbose", Required = false, HelpText = "Enable verbose messages.", Default = false)]
            public bool Verbose { get; set; }

            [Option('e', "noerror", Required = false, HelpText = "Disable WARN and ERROR messages.", Default = false)]
            public bool NoError { get; set; }

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

            [Verb("count", HelpText = "Counts the number of references to each unique protein.")]
            public class Count : GlobalOptions
            {
                [Option('m', "merge", Required = false, Default = false, HelpText = "Set to output a single list of all proteins in all files.\nIf not set, makes a list for every file and doesn't count references from other files.")]
                public bool Merge { get; set; }

                [Option('p', "exclude", Required = false, HelpText = "List of proteins to exclude from the count.")]
                public IEnumerable<string>? ExcludedProteins { get; set; }

                public bool Verbose { get; set; }
                public bool NoError { get; set; }
                public IEnumerable<char>? Delimiters { get; set; }
                public char? RowDelimiter { get; set; }
                public int SampleSize { get; set; }
                public int SafeRowCount { get; set; }
                public IEnumerable<string>? Files { get; set; }
            }

            [Verb("intersect", HelpText = "Intersects all files into a single list of unique proteins.")]
            public class Intersect : GlobalOptions
            {
                [Option('p', "exclude", Required = false, HelpText = "List of proteins to exclude from the intersection.")]
                public IEnumerable<string>? ExcludedProteins { get; set; }

                public bool Verbose { get; set; }
                public bool NoError { get; set; }
                public IEnumerable<char>? Delimiters { get; set; }
                public char? RowDelimiter { get; set; }
                public int SampleSize { get; set; }
                public int SafeRowCount { get; set; }
                public IEnumerable<string>? Files { get; set; }
            }
        }

        public static int Main(string[] args)
        {
            return new Parser(p =>
            {
                p.EnableDashDash = true;
                p.AutoHelp = true;
                p.HelpWriter = Console.Error;
            }).ParseArguments<GlobalOptions.Count, GlobalOptions.Intersect>(args)
            .MapResult(
              (GlobalOptions.Count options) => new ProteinCompare(options).StartCount(options),
              (GlobalOptions.Intersect options) => new ProteinCompare(options).StartIntersect(options),
              _ => 1);
        }

        private GlobalOptions options;
        private CsvTable[] tables;
        private int proteinCount;

        public ProteinCompare(GlobalOptions options)
        {
            this.options = options;
            tables = Array.Empty<CsvTable>();
            proteinCount = 0;

            LogManager.Setup().LoadConfiguration(builder =>
            {
                // StdOut for <= Warn
                builder.ForLogger().FilterLevels(options.Verbose ? LogLevel.Trace : LogLevel.Info, options.NoError ? LogLevel.Info : LogLevel.Warn).WriteTo(new ColoredConsoleTarget()
                {
                    StdErr = false,
                    AutoFlush = true,
                    Layout = Layout.FromMethod(evt =>
                {
                    return string.Format("{0}|{1}: {2}", evt.Level.Name.ToUpperInvariant(), evt.LoggerName, evt.FormattedMessage);
                })
                });

                // StdErr for >= Error
                builder.ForLogger().FilterMinLevel(options.NoError ? LogLevel.Fatal : LogLevel.Error).WriteTo(new ColoredConsoleTarget()
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
    
        public int Load()
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

            this.tables = tables.ToArray();
            logger.Info("{file_count} file(s) loaded.", tables.Count);

            int proteinCount = 0;

            for (int i = 0; i < tables.Count; i++)
            {
                logger.Info("Converting proteins: table {progress}/{total}", i + 1, tables.Count);
                foreach (var row in tables[i].Rows)
                {
                    if (row.Values.Length > 0)
                    {
                        if (ProteinDetector.GetProtein(row.Values[0], out string protein))
                        {
                            row.AttachedData = protein;
                            proteinCount++;
                        }
                    }
                }
            }

            this.proteinCount = proteinCount;
            logger.Info("Attached {protein_cont} protein(s).", proteinCount);

            return 0;
        }

        public int StartCount(GlobalOptions.Count options)
        {
            int exit = Load();
            if (exit != 0) return exit;

            return 0;
        }

        public int StartIntersect(GlobalOptions.Intersect options)
        {
            int exit = Load();
            if (exit != 0) return exit;

            return 0;
        }
    }
}