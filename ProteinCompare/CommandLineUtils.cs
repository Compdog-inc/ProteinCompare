using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public static class CommandLineUtils
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static string[] ReadCommandLine(string command)
        {
            List<string> args = new();
            StringBuilder arg = new();

            bool inArg = false;
            bool inQuote = false;

            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == '\\' && i < command.Length - 1) // handle escapes first
                {
                    switch (command[i + 1])
                    {
                        case '"':
                            arg.Append('"');
                            i++;
                            break;
                        case '\\':
                            arg.Append('\\');
                            i++;
                            break;
                        case 'n':
                            arg.Append('\n');
                            i++;
                            break;
                        default:
                            logger.Error("Unknown escape sequence {escape}.", command[i + 1]);
                            break;
                    }
                }
                else if (!inArg)
                {
                    inArg = true;
                    if (command[i] == '"')
                    {
                        inQuote = true;
                    }
                    else
                    {
                        arg.Append(command[i]);
                    }
                }
                else if (command[i] == '"')
                {
                    inQuote = !inQuote;
                }
                else if (command[i] == ' ' && !inQuote)
                {
                    args.Add(arg.ToString());
                    arg.Clear();
                }
                else
                {
                    arg.Append(command[i]);
                }
            }

            if(arg.Length > 0)
            {
                args.Add(arg.ToString());
            }

            return args.ToArray();
        }
    }
}
