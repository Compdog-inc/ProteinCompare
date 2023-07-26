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
                        if (protein_parts[0].Length <= 10)
                        {
                            protein = protein_parts[0];
                            return true;
                        } else
                        {
                            logger.Trace("RBPmap fail: suspiciously long protein {protein}", protein_parts[0]);
                        }
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
                    if (parts[1].Length <= 10)
                    {
                        protein = parts[1];
                        return true;
                    }
                    else
                    {
                        logger.Trace("catrapid fail: suspiciously long protein {protein}", parts[1]);
                    }
                } else
                {
                    logger.Trace("catrapid fail: invalid split '_'");
                }
            } else if(text.All(char.IsLetterOrDigit))
            {
                // alphanumeric
                if (text.Length <= 10)
                {
                    protein = text;
                    return true;
                }
                else
                {
                    logger.Trace("alphanumeric fail: suspiciously long protein {protein}", text);
                }
            }
            return false;
        }
    }
}
