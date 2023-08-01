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
                        var csv = new CsvBuilder()
                            .AddColumn("list_id", CsvType.Number)
                            .AddColumn("protein", CsvType.String)
                            .AddColumn("count", CsvType.Number)
                            .SetHeader(true);

                        for (int i = 0; i < lists.Length; i++)
                        {
                            var sorted = from entry in lists[i] orderby entry.Value descending select entry;
                            foreach (var pair in sorted)
                            {
                                csv.AddToRow((long)i).AddToRow(pair.Key).AddToRow((long)pair.Value).PushRow();
                            }
                        }

                        CsvWriter.WriteToFile(output, csv.ToTable(), new CsvDialect(',', '"', '"'), '\n');
                    }
                    break;
                case OutputFormat.Tsv:
                    {
                        var csv = new CsvBuilder()
                            .AddColumn("list_id", CsvType.Number)
                            .AddColumn("protein", CsvType.String)
                            .AddColumn("count", CsvType.Number)
                            .SetHeader(true);

                        for (int i = 0; i < lists.Length; i++)
                        {
                            var sorted = from entry in lists[i] orderby entry.Value descending select entry;
                            foreach (var pair in sorted)
                            {
                                csv.AddToRow((long)i).AddToRow(pair.Key).AddToRow((long)pair.Value).PushRow();
                            }
                        }

                        CsvWriter.WriteToFile(output, csv.ToTable(), new CsvDialect('\t', null, '"'), '\n');
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
            switch (format)
            {
                case OutputFormat.Readable:
                    {
                        using FileStream fs = File.Create(output);
                        using StreamWriter sw = new(fs);

                        for (int i = 0; i < proteins.Length; i++)
                        {
                            sw.Write(proteins[i]);
                            if (i < proteins.Length - 1)
                                sw.Write(",");
                        }
                    }
                    break;
                case OutputFormat.TextList:
                    {
                        using FileStream fs = File.Create(output);
                        using StreamWriter sw = new(fs);

                        for (int i = 0; i < proteins.Length; i++)
                        {
                            sw.Write(proteins[i]);
                            if (i < proteins.Length - 1)
                                sw.Write(",");
                        }
                    }
                    break;
                case OutputFormat.Json:
                    {
                        using FileStream fs = File.Create(output);
                        using StreamWriter sw = new(fs);
                        using JsonTextWriter json = new(sw);

                        json.WriteStartArray();
                        for (int i = 0; i < proteins.Length; i++)
                        {
                            json.WriteValue(proteins[i]);
                        }
                        json.WriteEndArray();
                    }
                    break;
                case OutputFormat.Csv:
                    {
                        var csv = new CsvBuilder()
                            .AddColumn("protein", CsvType.String)
                            .SetHeader(true);
                        for (int i = 0; i < proteins.Length; i++)
                        {
                            csv.AddToRow(proteins[i]).PushRow();
                        }

                        CsvWriter.WriteToFile(output, csv.ToTable(), new CsvDialect(',', '"', '"'), '\n');
                    }
                    break;
                case OutputFormat.Tsv:
                    {
                        var csv = new CsvBuilder()
                            .AddColumn("protein", CsvType.String)
                            .SetHeader(true);
                        for (int i = 0; i < proteins.Length; i++)
                        {
                            csv.AddToRow(proteins[i]).PushRow();
                        }

                        CsvWriter.WriteToFile(output, csv.ToTable(), new CsvDialect('\t', null, '"'), '\n');
                    }
                    break;
                default:
                    logger.Fatal("Unknown output format {format}", format);
                    return;
            }
            logger.Info("Finished writing to {file} with format {format}", Path.GetFileName(output), format);
        }

        public static void PrintCsvTables(CsvTable[] tables)
        {
            for (int i = 0; i < tables.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("==== Table #" + (i + 1) + " ====");

                if (tables[i].HasHeader)
                {
                    // print header
                    for (int k = 0; k < tables[i].Rows[0].Values.Length; k++)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write(tables[i].Rows[0].Values[k].ToString());

                        if (k < tables[i].Rows[0].Values.Length - 1)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.Write("\t");
                        }
                    }

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine();
                }

                foreach (var row in tables[i].HasHeader ? tables[i].Rows[1..] : tables[i].Rows)
                {
                    for (int k = 0; k < row.Values.Length; k++)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write('"');

                        if (row.Values[k].IsList)
                        {
                            var list = row.Values[k].ToStringList();
                            for (int j = 0; j < list.Length; j++)
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write(list[j]);
                                if (j < list.Length - 1)
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkGray;
                                    Console.Write(',');
                                }
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(row.Values[k].ToString());
                        }

                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write('"');

                        if (k < row.Values.Length - 1)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.Write("\t");
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine();
                }
            }
            Console.Out.Flush();
        }

        public static void WriteCsvTables(CsvTable[] tables, string output, OutputFormat format)
        {
            switch (format)
            {
                case OutputFormat.Readable:
                    {
                        using FileStream fs = File.Create(output);
                        using StreamWriter sw = new(fs);

                        for (int i = 0; i < tables.Length; i++)
                        {
                            sw.WriteLine("==== Table #" + (i + 1) + " ====");

                            if (tables[i].HasHeader)
                            {
                                // print header
                                for (int k = 0; k < tables[i].Rows[0].Values.Length; k++)
                                {
                                    sw.Write(tables[i].Rows[0].Values[k].ToString());

                                    if (k < tables[i].Rows[0].Values.Length - 1)
                                    {
                                        sw.Write("\t");
                                    }
                                }

                                sw.WriteLine();
                            }

                            foreach (var row in tables[i].HasHeader ? tables[i].Rows[1..] : tables[i].Rows)
                            {
                                for (int k = 0; k < row.Values.Length; k++)
                                {
                                    sw.Write('"');

                                    if (row.Values[k].IsList)
                                    {
                                        var list = row.Values[k].ToStringList();
                                        for (int j = 0; j < list.Length; j++)
                                        {
                                            sw.Write(list[j]);
                                            if (j < list.Length - 1)
                                            {
                                                sw.Write(',');
                                            }
                                        }
                                    }
                                    else
                                    {
                                        sw.Write(row.Values[k].ToString());
                                    }

                                    sw.Write('"');

                                    if (k < row.Values.Length - 1)
                                    {
                                        sw.Write("\t");
                                    }
                                }
                                sw.WriteLine();
                            }
                        }
                    }
                    break;
                case OutputFormat.TextList:
                    {
                        using FileStream fs = File.Create(output);
                        using StreamWriter sw = new(fs);

                        for (int i = 0; i < tables.Length; i++)
                        {
                            sw.WriteLine("Table:" + (i + 1));

                            if (tables[i].HasHeader)
                            {
                                // print header
                                for (int k = 0; k < tables[i].Rows[0].Values.Length; k++)
                                {
                                    sw.Write(tables[i].Rows[0].Values[k].ToString());

                                    if (k < tables[i].Rows[0].Values.Length - 1)
                                    {
                                        sw.Write(",");
                                    }
                                }

                                sw.WriteLine();
                            }

                            foreach (var row in tables[i].HasHeader ? tables[i].Rows[1..] : tables[i].Rows)
                            {
                                for (int k = 0; k < row.Values.Length; k++)
                                {
                                    sw.Write('"');

                                    if (row.Values[k].IsList)
                                    {
                                        var list = row.Values[k].ToStringList();
                                        for (int j = 0; j < list.Length; j++)
                                        {
                                            sw.Write(list[j]);
                                            if (j < list.Length - 1)
                                            {
                                                sw.Write(',');
                                            }
                                        }
                                    }
                                    else
                                    {
                                        sw.Write(row.Values[k].ToString());
                                    }

                                    sw.Write('"');

                                    if (k < row.Values.Length - 1)
                                    {
                                        sw.Write(",");
                                    }
                                }
                                sw.Write(';');
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
                        for (int i = 0; i < tables.Length; i++)
                        {
                            json.WriteStartObject();
                            if (tables[i].HasHeader)
                            {
                                // print header
                                json.WritePropertyName("header");
                                json.WriteStartArray();
                                for (int k = 0; k < tables[i].Rows[0].Values.Length; k++)
                                {
                                    json.WriteValue(tables[i].Rows[0].Values[k].ToString());
                                }
                                json.WriteEndArray();
                            }

                            json.WritePropertyName("rows");
                            json.WriteStartArray();
                            foreach (var row in tables[i].HasHeader ? tables[i].Rows[1..] : tables[i].Rows)
                            {
                                for (int k = 0; k < row.Values.Length; k++)
                                {
                                    if (row.Values[k].IsList)
                                    {
                                        json.WriteStartArray();
                                        var list = row.Values[k].ToStringList();
                                        for (int j = 0; j < list.Length; j++)
                                        {
                                            json.WriteValue(list[j]);
                                        }
                                        json.WriteEndArray();
                                    }
                                    else
                                    {
                                        json.WriteValue(row.Values[k].ToString());
                                    }
                                }
                            }
                            json.WriteEndArray();
                            json.WriteEndObject();
                        }
                        json.WriteEndArray();
                    }
                    break;
                case OutputFormat.Csv:
                    {
                        CsvWriter.WriteToFile(output, tables, new CsvDialect(',', '"', '"'), '\n');
                    }
                    break;
                case OutputFormat.Tsv:
                    {
                        CsvWriter.WriteToFile(output, tables, new CsvDialect('\t', null, '"'), '\n');
                    }
                    break;
                default:
                    logger.Fatal("Unknown output format {format}", format);
                    return;
            }
            logger.Info("Finished writing to {file} with format {format}", Path.GetFileName(output), format);
        }
    }
}
