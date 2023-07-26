using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public class CsvTable
    {
        public CsvColumn[] Columns { get; set; }
        public CsvRow[] Rows { get; set; }

        public CsvTable(CsvColumn[] columns, CsvRow[] rows)
        {
            Columns = columns;
            Rows = rows;
        }
    }

    public class CsvRow
    {
        public int Index { get; set; }
        public CsvValue[] Values { get; set; }

        public CsvRow(int index, CsvValue[] values)
        {
            Index = index;
            Values = values;
        }
    }

    public struct CsvValue
    {
        public string Value { get; set; }
        public int ColumnIndex { get; set; }
        public CsvType ColumnType { get; set; }

        public CsvValue(string value, int columnIndex, CsvType columnType)
        {
            Value = value;
            ColumnIndex = columnIndex;
            ColumnType = columnType;
        }

        public readonly bool ToBoolean()
        {
            if (CsvParser.TryParseBoolean(Value, out bool result))
                return result;
            else
                throw new InvalidCastException("Can't convert " + ColumnType + " to Boolean");
        }

        public readonly long ToLong()
        {
            if (CsvParser.TryParseNumber(Value, out long result))
                return result;
            else
                throw new InvalidCastException("Can't convert " + ColumnType + " to Number");
        }

        public readonly double ToDouble()
        {
            if (CsvParser.TryParseDouble(Value, out double result))
                return result;
            else
                throw new InvalidCastException("Can't convert " + ColumnType + " to Double");
        }

        public readonly TimeOnly ToTime()
        {
            if (CsvParser.TryParseTime(Value, out TimeOnly result))
                return result;
            else
                throw new InvalidCastException("Can't convert " + ColumnType + " to Time");
        }

        public readonly DateOnly ToDate()
        {
            if (CsvParser.TryParseDate(Value, out DateOnly result))
                return result;
            else
                throw new InvalidCastException("Can't convert " + ColumnType + " to Date");
        }

        public readonly DateTime ToDateTime()
        {
            if (CsvParser.TryParseTimestamp(Value, out DateTime result))
                return result;
            else
                throw new InvalidCastException("Can't convert " + ColumnType + " to Timestamp");
        }

        public override readonly string ToString()
        {
            return Value;
        }

        public static implicit operator bool(CsvValue v) => v.ToBoolean();
        public static implicit operator long(CsvValue v) => v.ToLong();
        public static implicit operator double(CsvValue v) => v.ToDouble();
        public static implicit operator TimeOnly(CsvValue v) => v.ToTime();
        public static implicit operator DateOnly(CsvValue v) => v.ToDate();
        public static implicit operator DateTime(CsvValue v) => v.ToDateTime();
        public static implicit operator string(CsvValue v) => v.Value;
    }
}
