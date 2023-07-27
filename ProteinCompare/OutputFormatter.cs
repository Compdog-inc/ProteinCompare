using NLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public enum OutputFormat
    {
        Readable,
        TextList,
        Json,
        Csv,
        Tsv
    }

    public static class OutputFormatter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void PrintProteinCounts(Dictionary<string, uint>[] lists)
        {
            for (int i = 0; i < lists.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("==== List #" + (i + 1) + " ====");
                var sorted = from entry in lists[i] orderby entry.Value descending select entry;
                foreach (var pair in sorted)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(pair.Key);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(pair.Key.Length < 8 ? "\t\t" : "\t");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(pair.Value);
                }
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            Console.Out.Flush();
        }

        public static void WriteProteinCounts(Dictionary<string, uint>[] lists, string output, OutputFormat format)
        {
            switch (format)
            {
                case OutputFormat.Readable:
                    {
                        using FileStream fs = File.Create(output);
                        using StreamWriter sw = new(fs);

                        for (int i = 0; i < lists.Length; i++)
                        {
                            sw.WriteLine("==== List #" + (i + 1) + " ====");
                            var sorted = from entry in lists[i] orderby entry.Value descending select entry;
                            foreach (var pair in sorted)
                            {
                                sw.WriteLine(pair.Key + (pair.Key.Length < 8 ? "\t\t" : "\t") + pair.Value);
                            }
                        }
                    }
                    break;
                case OutputFormat.TextList:
                    {
                        using FileStream fs = File.Create(output);
                        using StreamWriter sw = new(fs);

                        for (int i = 0; i < lists.Length; i++)
                        {
                            sw.WriteLine("List:" + (i + 1));
                            var sorted = from entry in lists[i] orderby entry.Value descending select entry;
                            foreach (var pair in sorted)
                            {
                                sw.Write(pair.Key + ":" + pair.Value + ",");
                            }
                            sw.WriteLine();
                        }
                    }
                    break;
                case OutputFormat.Json:
                    {
                        using FileStream fs = File.Create(output);
                        using StreamWriter sw = new(fs);
                        using JsonTextWriter json = new(sw);

                        json.WriteStartArray();
                        for (int i = 0; i < lists.Length; i++)
                        {
                            json.WriteStartObject();
                            var sorted = from entry in lists[i] orderby entry.Value descending select entry;
                            foreach (var pair in sorted)
                            {
                                json.WritePropertyName(pair.Key);
                                json.WriteValue(pair.Value);
                            }
                            json.WriteEndObject();
                        }
                        json.WriteEndArray();
                    }
                    break;
                case OutputFormat.Csv:
                    {
                        using FileStream fs = File.Create(output);
                        using StreamWriter sw = new(fs);
                        
                    }
                    break;
                default:
                    logger.Fatal("Unknown output format {format}", format);
                    return;
            }
            logger.Info("Finished writing to {file} with format {format}", Path.GetFileName(output), format);
        }

        public static void PrintProteinList(string[] proteins)
        {
            for (int i = 0; i < proteins.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(proteins[i]);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                if (i < proteins.Length - 1)
                    Console.Write(",");
            }
            Console.Out.Flush();
        }

        public static void WriteProteinList(string[] proteins, string output, OutputFormat format)
        {
            for (int i = 0; i < proteins.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(proteins[i]);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                if (i < proteins.Length - 1)
                    Console.Write(",");
            }
            Console.Out.Flush();
        }
    }
}
