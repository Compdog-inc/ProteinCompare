using NLog;
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
        public bool HasHeader { get; set; }

        public CsvTable(CsvColumn[] columns, CsvRow[] rows, bool hasHeader)
        {
            Columns = columns;
            Rows = rows;
            HasHeader = hasHeader;
        }
    }

    public class CsvRow
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public int Index { get; set; }
        public CsvValue[] Values { get; set; }
        public object? AttachedData { get; set; }

        public CsvRow(int index, CsvValue[] values)
        {
            Index = index;
            Values = values;
        }

        /// <summary>
        /// <para>Merges two rows with same column types into lists of values. Attached data and index are not merged.</para>
        /// <para>If the column types are different - both are added, increasing the final column count.</para>
        /// </summary>
        /// <param name="other">Other row to merge with</param>
        /// <param name="mergeCallback">Callback to handle value merges (source, other, columnIndex) => mergedValue</param>
        /// <param name="attachedDataMergeCallback">Callback to handle attached data merges (source, other) => merged</param>
        public CsvRow Merge(CsvRow other, Func<CsvValue, CsvValue, int, CsvValue> mergeCallback, Func<object?, object?, object?> attachedDataMergeCallback)
        {
            List<CsvValue> values = new(Math.Max(Values.Length, other.Values.Length));
            int index = 0;
            for (int i = 0; i < Math.Max(Values.Length, other.Values.Length); i++)
            {
                if (i < Values.Length && i < other.Values.Length) // merge
                {
                    if (Values[i].ColumnType == other.Values[i].ColumnType) // same column type
                    {
                        values.Add(mergeCallback(Values[i], other.Values[i], index));
                    } else
                    {
                        values.Add(new(Values[i].Value, index, Values[i].ColumnType)); // add source
                        index++;
                        values.Add(new(other.Values[i].Value, index, other.Values[i].ColumnType)); // add other
                    }
                }
                else if (i < Values.Length) // copy source
                {
                    values.Add(new CsvValue(Values[i].Values, index, Values[i].ColumnType));
                }
                else if (i < other.Values.Length) // copy other
                {
                    values.Add(new CsvValue(other.Values[i].Values, index, other.Values[i].ColumnType));
                }
                index++;
            }

            return new CsvRow(Index, values.ToArray())
            {
                AttachedData = attachedDataMergeCallback(AttachedData, other.AttachedData)
            };
        }

        public CsvRow Merge(CsvRow other)
        {
            return Merge(other, (a, b, i) => new CsvValue(a.Values.Concat(b.Values).ToArray(), i, a.ColumnType), (a, b) => a);
        }
    }

    public struct CsvValue
    {
        public readonly string Value
        {
            get => Values[0]; set
            {
                Values[0] = value;
            }
        }
        public string[] Values { get; set; }
        public int ColumnIndex { get; set; }
        public CsvType ColumnType { get; set; }
        public bool IsList { get; }

        public CsvValue(string value, int columnIndex, CsvType columnType)
        {
            Values = new string[1] { value };
            ColumnIndex = columnIndex;
            ColumnType = columnType;
            IsList = false;
        }

        public CsvValue(string[] values, int columnIndex, CsvType columnType)
        {
            Values = values;
            ColumnIndex = columnIndex;
            ColumnType = columnType;
            IsList = true;
        }

        public readonly CsvValue WithList()
        {
            return new CsvValue(new string[1] { Value }, ColumnIndex, ColumnType);
        }

        public readonly bool ToBoolean()
        {
            if (CsvParser.TryParseBoolean(Value, out bool result))
                return result;
            else
                throw new InvalidCastException("Can't convert " + ColumnType + " to Boolean");
        }

        public readonly bool[] ToBooleanList()
        {
            if (!IsList) throw new InvalidCastException("Can't convert single value to list");

            bool[] result = new bool[Values.Length];

            for (int i = 0; i < result.Length; i++)
            {
                if (CsvParser.TryParseBoolean(Value, out bool tmp))
                    result[i] = tmp;
                else
                    throw new InvalidCastException("Can't convert " + ColumnType + " to Boolean");
            }

            return result;
        }

        public readonly long ToLong()
        {
            if (CsvParser.TryParseNumber(Value, out long result))
                return result;
            else
                throw new InvalidCastException("Can't convert " + ColumnType + " to Number");
        }

        public readonly long[] ToLongList()
        {
            if (!IsList) throw new InvalidCastException("Can't convert single value to list");

            long[] result = new long[Values.Length];

            for (int i = 0; i < result.Length; i++)
            {
                if (CsvParser.TryParseNumber(Value, out long tmp))
                    result[i] = tmp;
                else
                    throw new InvalidCastException("Can't convert " + ColumnType + " to Number");
            }

            return result;
        }

        public readonly double ToDouble()
        {
            if (CsvParser.TryParseDouble(Value, out double result))
                return result;
            else
                throw new InvalidCastException("Can't convert " + ColumnType + " to Double");
        }

        public readonly double[] ToDoubleList()
        {
            if (!IsList) throw new InvalidCastException("Can't convert single value to list");

            double[] result = new double[Values.Length];

            for (int i = 0; i < result.Length; i++)
            {
                if (CsvParser.TryParseDouble(Value, out double tmp))
                    result[i] = tmp;
                else
                    throw new InvalidCastException("Can't convert " + ColumnType + " to Double");
            }

            return result;
        }

        public readonly TimeOnly ToTime()
        {
            if (CsvParser.TryParseTime(Value, out TimeOnly result))
                return result;
            else
                throw new InvalidCastException("Can't convert " + ColumnType + " to Time");
        }

        public readonly TimeOnly[] ToTimeList()
        {
            if (!IsList) throw new InvalidCastException("Can't convert single value to list");

            TimeOnly[] result = new TimeOnly[Values.Length];

            for (int i = 0; i < result.Length; i++)
            {
                if (CsvParser.TryParseTime(Value, out TimeOnly tmp))
                    result[i] = tmp;
                else
                    throw new InvalidCastException("Can't convert " + ColumnType + " to Time");
            }

            return result;
        }

        public readonly DateOnly ToDate()
        {
            if (CsvParser.TryParseDate(Value, out DateOnly result))
                return result;
            else
                throw new InvalidCastException("Can't convert " + ColumnType + " to Date");
        }

        public readonly DateOnly[] ToDateList()
        {
            if (!IsList) throw new InvalidCastException("Can't convert single value to list");

            DateOnly[] result = new DateOnly[Values.Length];

            for (int i = 0; i < result.Length; i++)
            {
                if (CsvParser.TryParseDate(Value, out DateOnly tmp))
                    result[i] = tmp;
                else
                    throw new InvalidCastException("Can't convert " + ColumnType + " to Date");
            }

            return result;
        }

        public readonly DateTime ToDateTime()
        {
            if (CsvParser.TryParseTimestamp(Value, out DateTime result))
                return result;
            else
                throw new InvalidCastException("Can't convert " + ColumnType + " to Timestamp");
        }

        public readonly DateTime[] ToDateTimeList()
        {
            if (!IsList) throw new InvalidCastException("Can't convert single value to list");

            DateTime[] result = new DateTime[Values.Length];

            for (int i = 0; i < result.Length; i++)
            {
                if (CsvParser.TryParseTimestamp(Value, out DateTime tmp))
                    result[i] = tmp;
                else
                    throw new InvalidCastException("Can't convert " + ColumnType + " to Timestamp");
            }

            return result;
        }

        public override readonly string ToString()
        {
            return Value;
        }

        public readonly string[] ToStringList()
        {
            if (!IsList) throw new InvalidCastException("Can't convert single value to list");

            return Values;
        }

        public static implicit operator bool(CsvValue v) => v.ToBoolean();
        public static implicit operator long(CsvValue v) => v.ToLong();
        public static implicit operator double(CsvValue v) => v.ToDouble();
        public static implicit operator TimeOnly(CsvValue v) => v.ToTime();
        public static implicit operator DateOnly(CsvValue v) => v.ToDate();
        public static implicit operator DateTime(CsvValue v) => v.ToDateTime();
        public static implicit operator string(CsvValue v) => v.Value;

        public static implicit operator bool[](CsvValue v) => v.ToBooleanList();
        public static implicit operator long[](CsvValue v) => v.ToLongList();
        public static implicit operator double[](CsvValue v) => v.ToDoubleList();
        public static implicit operator TimeOnly[](CsvValue v) => v.ToTimeList();
        public static implicit operator DateOnly[](CsvValue v) => v.ToDateList();
        public static implicit operator DateTime[](CsvValue v) => v.ToDateTimeList();
        public static implicit operator string[](CsvValue v) => v.Values;
    }
}
