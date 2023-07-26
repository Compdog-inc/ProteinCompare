using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public static class ProteinDetector
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static bool GetProtein(string text, out string protein)
        {
            // TODO: add protein value checking
            protein = string.Empty;

            if (text.StartsWith("Protein: ", StringComparison.InvariantCultureIgnoreCase))
            {
                // RBPmap
                var parts = text.Split(':', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if(parts.Length > 1)
                {
                    var protein_parts = parts[1].Split('(', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if(protein_parts.Length > 0)
                    {
                        protein = protein_parts[0];
                        return true;
                    } else
                    {
                        logger.Trace("RBPmap fail: invalid split '('");
                    }
                } else
                {
                    logger.Trace("RBPmap fail: invalid split ':'");
                }
            } else if (text.Contains('_'))
            {
                // catrapid
                var parts = text.Split('_', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if(parts.Length > 1)
                {
                    protein = parts[1];
                    return true;
                } else
                {
                    logger.Trace("catrapid fail: invalid split '_'");
                }
            } else
            {
                protein = text;
                return true;
            }
            return false;
        }
    }
}
