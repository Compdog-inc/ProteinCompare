using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public class CsvColumn
    {
        public string? Name { get; set; }
        public CsvType Type { get; set; }

        public CsvColumn(CsvType type)
        {
            Type = type;
        }

        public CsvColumn(string name, CsvType type)
        {
            Name = name;
            Type = type;
        }
    }
}
