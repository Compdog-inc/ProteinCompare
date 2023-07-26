using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public static class OutputFormatter
    {
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
    }
}
