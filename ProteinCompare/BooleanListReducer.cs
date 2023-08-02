using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public class BooleanListReducer : ICsvListReducer
    {
        public static CsvValue ToUnique(CsvValue value)
        {
            return new CsvValue(value.Values.Distinct().ToArray(), value.ColumnIndex, value.ColumnType, value.IsList, false); // still valid list so not actually reduced
        }

        public static CsvValue ToRange(CsvValue value, double jumpInterval)
        {
            throw new NotImplementedException("Cannot numerically reduce BooleanList.");
        }

        public static CsvValue ToStartEnd(CsvValue value)
        {
            throw new NotImplementedException("Cannot numerically reduce BooleanList.");
        }

        public static CsvValue ToStep(CsvValue value, double stepInterval)
        {
            throw new NotImplementedException("Cannot numerically reduce BooleanList.");
        }
    }
}
