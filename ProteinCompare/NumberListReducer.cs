using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public class NumberListReducer : ICsvListReducer
    {
        public static CsvValue ToUnique(CsvValue value)
        {
            return new CsvValue(value.Values.Distinct().ToArray(), value.ColumnIndex, value.ColumnType, value.IsList, false); // still valid list so not actually reduced
        }

        public static CsvValue ToRange(CsvValue value, double jumpInterval)
        {
            if (value.ColumnType != CsvType.Number) throw new ArgumentException("Column type is not Number", nameof(value));
            long[] values = value.ToLongList();
            Array.Sort(values);

            List<(long, long)> ranges = new(); // min, max
            long? currentMin = null;
            long? currentMax = null;

            for (int i = 0; i < values.Length; i++)
            {
                if (currentMin == null || currentMax == null)
                    currentMin = currentMax = values[i];
                else
                {
                    long min = values[i] < (long)currentMin ? values[i] : (long)currentMin;
                    long max = values[i] > (long)currentMax ? values[i] : (long)currentMax;

                    if (Math.Abs(min - max) >= jumpInterval) // value pulled away
                    {
                        ranges.Add(((long)currentMin, (long)currentMax));
                        currentMin = null;
                        currentMax = null;
                    }
                    else
                    {
                        currentMin = min;
                        currentMax = max;
                    }
                }
            }

            if(currentMin != null && currentMax != null)
            {
                ranges.Add(((long)currentMin, (long)currentMax));
            }

            string[] str = new string[ranges.Count];
            for(int i = 0; i < ranges.Count; i++)
            {
                str[i] =
                    ranges[i].Item1 == ranges[i].Item2 ? CsvExtensions.SerializeObject(ranges[i].Item1).Item2 :
                    CsvExtensions.SerializeObject(ranges[i].Item1).Item2 + " - " + CsvExtensions.SerializeObject(ranges[i].Item2).Item2;
            }

            return new CsvValue(str, value.ColumnIndex, value.ColumnType, value.IsList, true);
        }

        public static CsvValue ToStartEnd(CsvValue value)
        {
            if (value.ColumnType != CsvType.Number) throw new ArgumentException("Column type is not Number", nameof(value));
            long[] values = value.ToLongList();
            long? currentMin = null;
            long? currentMax = null;

            for (int i = 0; i < values.Length; i++)
            {
                if (currentMin == null || currentMax == null)
                    currentMin = currentMax = values[i];
                else
                {
                    currentMin = values[i] < (long)currentMin ? values[i] : (long)currentMin;
                    currentMax = values[i] > (long)currentMax ? values[i] : (long)currentMax;
                }
            }

            return new CsvValue(
                            currentMin == currentMax ? new string[1] { CsvExtensions.SerializeObject(currentMin).Item2 } :
                            new string[2] { CsvExtensions.SerializeObject(currentMin).Item2, CsvExtensions.SerializeObject(currentMax).Item2 },
                            value.ColumnIndex, value.ColumnType, value.IsList, true);
        }

        public static CsvValue ToStep(CsvValue value, double stepInterval)
        {
            if (value.ColumnType != CsvType.Number) throw new ArgumentException("Column type is not Number", nameof(value));
            long[] values = value.ToLongList();
            Array.Sort(values);

            List<long> steps = new();
            long? lastValue = null;

            for (int i = 0; i < values.Length; i++)
            {
                if (lastValue == null)
                    lastValue = values[i];
                else if (Math.Abs((long)lastValue - values[i]) >= stepInterval) // value pulled away
                {
                    steps.Add((long)lastValue);
                    lastValue = values[i];
                }
            }

            if (lastValue != null)
            {
                steps.Add((long)lastValue);
            }

            string[] str = new string[steps.Count];
            for (int i = 0; i < steps.Count; i++)
            {
                str[i] = CsvExtensions.SerializeObject(steps[i]).Item2;
            }

            return new CsvValue(str, value.ColumnIndex, value.ColumnType, value.IsList, true);
        }
    }
}
