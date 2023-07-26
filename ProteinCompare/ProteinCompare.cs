using CommandLine;
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

            [Value(0, MetaName = "[...files]", MetaValue = "<string>", HelpText = "List of paths to protein files (separated by spaces)")]
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
            logger.Trace(FileParser.Parse(options.Files!).Aggregate((a, b) => a + ", " + b));

            return 0;
        }
    }
}