using CommandLine;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using ShellProgressBar;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ProteinCompare
{
    class StringComparer : IEqualityComparer<string>
    {
        public bool IgnoreCase { get; set; }

        public StringComparer(bool ignoreCase)
        {
            this.IgnoreCase = ignoreCase;
        }

        public bool Equals(string? x, string? y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null) return false;

            return x.Equals(y, IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            return obj.ToUpperInvariant().GetHashCode();
        }
    }

    class ProteinCompare
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public interface GlobalOptions
        {
            [Option('v', "verbose", Required = false, HelpText = "Enable verbose messages.", Default = false)]
            public bool Verbose { get; set; }

            [Option('e', "no-error", Required = false, HelpText = "Disable WARN and ERROR messages.", Default = false)]
            public bool NoError { get; set; }

            [Option('d', "delimiters", Required = false, MetaValue = "<char_1> <char_2> ...", HelpText = "Add custom CSV column delimiters used for detection.")]
            public IEnumerable<char>? Delimiters { get; set; }

            [Option('r', "row-delimiter", Required = false, MetaValue = "<char>", HelpText = "(Default: \\n) Custom row delimiter for parsing.")]
            public char? RowDelimiter { get; set; }

            [Option('s', "sample", Required = false, Default = 256, MetaValue = "<int:(row count)>", HelpText = "Set CSV table sample size in rows for detection.")]
            public int SampleSize { get; set; }

            [Option('h', "safe", Required = false, Default = 2, MetaValue = "<int:(row count)>", HelpText = "Set CSV table safe row count (allows detection errors).\nSet to the minimum header/abnormal row count.")]
            public int SafeRowCount { get; set; }

            [Option('o', "output", Required = false, MetaValue = "<string:(path)>", HelpText = "Path of output file to write. If not defined, output to stdout.")]
            public string? OutputFile { get; set; }

            [Option('l', "format", Required = false, MetaValue = "<Readable|TextList|Json|Csv|Tsv>", Default = OutputFormat.Readable, HelpText = "Set output file format. Ignored if output is not set.")]
            public OutputFormat OutFormat { get; set; }

            [Value(0, MetaName = "[...files]", MetaValue = "<string:(path)>", HelpText = "List of paths to protein files (separated by spaces and supports wildcards)")]
            public IEnumerable<string>? Files { get; set; }

            [Verb("count", HelpText = "Counts the number of references to each unique protein.")]
            public class Count : GlobalOptions
            {
                [Option('m', "merge", Required = false, Default = false, HelpText = "Set to output a single list of all proteins in all files.\nIf not set, makes a list for every file and doesn't count references from other files.")]
                public bool Merge { get; set; }

                [Option('c', "ignore-case", Required = false, Default = false, HelpText = "Set to ignore case when comparing proteins.")]
                public bool IgnoreCase { get; set; }

                [Option('k', "preprocess", Required = false, HelpText = "Set to preprocess input.", MetaValue = "<None|Intersect>", Default = Preprocessors.None)]
                public Preprocessors Preprocessor { get; set; }

                [Option('p', "exclude", Required = false, HelpText = "List of proteins to exclude from the count.", MetaValue = "<string:(protein)>")]
                public IEnumerable<string>? ExcludedProteins { get; set; }

                public bool Verbose { get; set; }
                public bool NoError { get; set; }
                public IEnumerable<char>? Delimiters { get; set; }
                public char? RowDelimiter { get; set; }
                public int SampleSize { get; set; }
                public int SafeRowCount { get; set; }
                public IEnumerable<string>? Files { get; set; }
                public string? OutputFile { get; set; }
                public OutputFormat OutFormat { get; set; }

                public enum Preprocessors
                {
                    None,
                    Intersect
                }
            }

            [Verb("intersect", HelpText = "Intersects all files into a single list of proteins.")]
            public class Intersect : GlobalOptions
            {
                [Option('c', "ignore-case", Required = false, Default = false, HelpText = "Set to ignore case when comparing proteins.")]
                public bool IgnoreCase { get; set; }

                [Option('p', "exclude", Required = false, HelpText = "List of proteins to exclude from the intersection.")]
                public IEnumerable<string>? ExcludedProteins { get; set; }

                public bool Verbose { get; set; }
                public bool NoError { get; set; }
                public IEnumerable<char>? Delimiters { get; set; }
                public char? RowDelimiter { get; set; }
                public int SampleSize { get; set; }
                public int SafeRowCount { get; set; }
                public IEnumerable<string>? Files { get; set; }
                public string? OutputFile { get; set; }
                public OutputFormat OutFormat { get; set; }
            }

            [Verb("meta", HelpText = "Attaches unique metadata to proteins.")]
            public class Meta : GlobalOptions
            {
                [Option('m', "merge", Required = false, Default = false, HelpText = "Set to output a single list of all proteins in all files.\nIf not set, makes a list for every file and doesn't use metadata from other files.")]
                public bool Merge { get; set; }

                [Option('k', "preprocess", Required = false, HelpText = "Set to preprocess input.", MetaValue = "<None|Count|Intersect>", Default = Preprocessors.None)]
                public IEnumerable<Preprocessors>? Preprocessor { get; set; }

                [Option('c', "ignore-case", Required = false, Default = false, HelpText = "Set to ignore case when comparing proteins.")]
                public bool IgnoreCase { get; set; }

                [Option('p', "exclude", Required = false, HelpText = "List of proteins to exclude from the list.")]
                public IEnumerable<string>? ExcludedProteins { get; set; }

                [Option('w', "columns", Required = false, HelpText = "List of column names to filter from the metadata.")]
                public IEnumerable<string>? FilteredColumns { get; set; }

                public bool Verbose { get; set; }
                public bool NoError { get; set; }
                public IEnumerable<char>? Delimiters { get; set; }
                public char? RowDelimiter { get; set; }
                public int SampleSize { get; set; }
                public int SafeRowCount { get; set; }
                public IEnumerable<string>? Files { get; set; }
                public string? OutputFile { get; set; }
                public OutputFormat OutFormat { get; set; }

                public enum Preprocessors
                {
                    None,
                    Count,
                    Intersect
                }
            }
        }

        public static int Main(string[] args)
        {
            return new Parser(p =>
            {
                p.CaseSensitive = false;
                p.CaseInsensitiveEnumValues = true;
                p.EnableDashDash = true;
                p.AutoHelp = true;
                p.HelpWriter = Console.Error;
            }).ParseArguments<GlobalOptions.Count, GlobalOptions.Intersect, GlobalOptions.Meta>(args)
            .MapResult(
              (GlobalOptions.Count options) => new ProteinCompare(options).StartCount(options),
              (GlobalOptions.Intersect options) => new ProteinCompare(options).StartIntersect(options),
              (GlobalOptions.Meta options) => new ProteinCompare(options).StartMeta(options),
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

            ProgressBar? pbar = null;
            if (options.NoError && !options.Verbose)
            {
                pbar = new ProgressBar(files.Length, "Reading input files", new ProgressBarOptions()
                {
                    ProgressBarOnBottom = true
                });
            }

            foreach (var file in files)
            {
                try
                {
                    tables.Add(CsvReader.ReadFile(file, options.RowDelimiter ?? '\n', options.SampleSize, options.SafeRowCount, options.Delimiters?.ToArray() ?? Array.Empty<char>()));
                    if(pbar == null)
                        logger.Info("Loaded table {path}", file);
                    pbar?.Tick("Reading input files - " + Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    if(pbar == null)
                        logger.Error(ex, "Error loading file {file}", file);
                    pbar?.Tick("Reading input files - " + Path.GetFileName(file));
                    pbar?.WriteErrorLine("Error loading " + Path.GetFileName(file) + ", " + ex.ToString());
                }
            }

            pbar?.Dispose();

            this.tables = tables.ToArray();
            logger.Info("{file_count} file(s) loaded.", tables.Count);

            int proteinCount = 0;

            if (options.NoError && !options.Verbose)
            {
                pbar = new ProgressBar(tables.Select(t => t.Rows.Length).Aggregate((a, b) => a + b), "Converting proteins", new ProgressBarOptions()
                {
                    ProgressBarOnBottom = true
                });
            }

            for (int i = 0; i < tables.Count; i++)
            {
                if(pbar == null)
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
                    pbar?.Tick("Converting proteins: table " + (i + 1) + "/" + tables.Count);
                }
            }

            pbar?.Dispose();

            this.proteinCount = proteinCount;
            logger.Info("Attached {protein_cont} protein(s).", proteinCount);

            return 0;
        }

        public int StartCount(GlobalOptions.Count options)
        {
            int exit = Load();
            if (exit != 0) return exit;

            Dictionary<string, uint>[] lists;

            if (options.Preprocessor == GlobalOptions.Count.Preprocessors.Intersect)
            {
                logger.Info("Preprocessing with Intersect");
                var intersected = ProteinIntersect.Run(tables, options.ExcludedProteins, options.IgnoreCase);
                logger.Info("Intersection completed with {protein_count} protein(s).", intersected.Length);
                if (options.Merge)
                {
                    lists = ProteinCounter.RunMergedList(tables, options.ExcludedProteins, options.IgnoreCase, intersected);
                }
                else
                {
                    lists = ProteinCounter.RunList(tables, options.ExcludedProteins, options.IgnoreCase, intersected);
                }
            }
            else
            {
                if (options.Merge)
                {
                    lists = ProteinCounter.RunMerged(tables, options.ExcludedProteins, options.IgnoreCase);
                }
                else
                {
                    lists = ProteinCounter.Run(tables, options.ExcludedProteins, options.IgnoreCase);
                }
            }

            logger.Info("Count completed with {list_count} list(s).", lists.Length);

            if (options.OutputFile == null)
                OutputFormatter.PrintProteinCounts(lists);
            else
                OutputFormatter.WriteProteinCounts(lists, options.OutputFile, options.OutFormat);

            return 0;
        }

        public int StartIntersect(GlobalOptions.Intersect options)
        {
            int exit = Load();
            if (exit != 0) return exit;

            var list = ProteinIntersect.Run(tables, options.ExcludedProteins, options.IgnoreCase);

            logger.Info("Intersection completed with {protein_count} protein(s).", list.Length);

            if (options.OutputFile == null)
                OutputFormatter.PrintProteinList(list);
            else
                OutputFormatter.WriteProteinList(list, options.OutputFile, options.OutFormat);

            return 0;
        }

        public int StartMeta(GlobalOptions.Meta options)
        {
            int exit = Load();
            if (exit != 0) return exit;

            string[]? filter = null;

            if (options.Preprocessor?.Contains(GlobalOptions.Meta.Preprocessors.Intersect) ?? false)
            {
                logger.Info("Preprocessing with Intersect");
                filter = ProteinIntersect.Run(tables, options.ExcludedProteins, options.IgnoreCase);
                logger.Info("Intersection completed with {protein_count} protein(s).", filter.Length);
            }

            CsvTable[] metatables;
            if (options.Merge)
            {
                metatables = ProteinMetaCollector.ReduceByProteinMerged(tables, options.ExcludedProteins, options.IgnoreCase, filter, options.FilteredColumns);
            } else
            {
                metatables = ProteinMetaCollector.ReduceByProtein(tables, options.ExcludedProteins, options.IgnoreCase, filter, options.FilteredColumns);
            }

            if (options.Preprocessor?.Contains(GlobalOptions.Meta.Preprocessors.Count) ?? false)
            {
                logger.Info("Preprocessing with Count");
                Dictionary<string, uint>[] lists;
                if (filter != null)
                {
                    if (options.Merge)
                    {
                        lists = ProteinCounter.RunMergedList(tables, options.ExcludedProteins, options.IgnoreCase, filter);
                    }
                    else
                    {
                        lists = ProteinCounter.RunList(tables, options.ExcludedProteins, options.IgnoreCase, filter);
                    }
                }
                else
                {
                    if (options.Merge)
                    {
                        lists = ProteinCounter.RunMerged(tables, options.ExcludedProteins, options.IgnoreCase);
                    }
                    else
                    {
                        lists = ProteinCounter.Run(tables, options.ExcludedProteins, options.IgnoreCase);
                    }
                }
                logger.Info("Count completed with {list_count} list(s).", lists.Length);

                if (metatables.Length != lists.Length)
                {
                    logger.Fatal("Count result mismatch list size: {list_size} != {table_count}.", lists.Length, metatables.Length);
                    return 1;
                }

                for (int i = 0; i < metatables.Length; i++)
                {
                    var tbl = lists[i].ToTable("Protein", "count");
                    metatables[i] = metatables[i].Merge(tbl, false); // don't combine count column
                }
            }

            logger.Info("Meta completed with {table_count} table(s).", metatables.Length);

            if (options.OutputFile == null)
                OutputFormatter.PrintCsvTables(metatables);
            else
                OutputFormatter.WriteCsvTables(metatables, options.OutputFile, options.OutFormat);

            return 0;
        }
    }
}
