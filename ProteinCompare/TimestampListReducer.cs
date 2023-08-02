using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public class TimestampListReducer : ICsvListReducer
    {
        public static CsvValue ToUnique(CsvValue value)
        {
            return new CsvValue(value.Values.Distinct().ToArray(), value.ColumnIndex, value.ColumnType, value.IsList, false); // still valid list so not actually reduced
        }

        public static CsvValue ToRange(CsvValue value, double jumpInterval)
        {
            if (value.ColumnType != CsvType.Timestamp) throw new ArgumentException("Column type is not Timestamp", nameof(value));
            DateTime[] values = value.ToDateTimeList();
            Array.Sort(values);

            List<(DateTime, DateTime)> ranges = new(); // min, max
            DateTime? currentMin = null;
            DateTime? currentMax = null;

            for (int i = 0; i < values.Length; i++)
            {
                if (currentMin == null || currentMax == null)
                    currentMin = currentMax = values[i];
                else
                {
                    DateTime min = values[i] < (DateTime)currentMin ? values[i] : (DateTime)currentMin;
                    DateTime max = values[i] > (DateTime)currentMax ? values[i] : (DateTime)currentMax;

                    if (Math.Abs(min.Ticks - max.Ticks) >= jumpInterval) // value pulled away
                    {
                        ranges.Add(((DateTime)currentMin, (DateTime)currentMax));
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

            if (currentMin != null && currentMax != null)
            {
                ranges.Add(((DateTime)currentMin, (DateTime)currentMax));
            }

            string[] str = new string[ranges.Count];
            for (int i = 0; i < ranges.Count; i++)
            {
                str[i] =
                    ranges[i].Item1 == ranges[i].Item2 ? CsvExtensions.SerializeObject(ranges[i].Item1).Item2 :
                    CsvExtensions.SerializeObject(ranges[i].Item1).Item2 + " - " + CsvExtensions.SerializeObject(ranges[i].Item2).Item2;
            }

            return new CsvValue(str, value.ColumnIndex, value.ColumnType, value.IsList, true);
        }

        public static CsvValue ToStartEnd(CsvValue value)
        {
            if (value.ColumnType != CsvType.Timestamp) throw new ArgumentException("Column type is not Timestamp", nameof(value));
            DateTime[] values = value.ToDateTimeList();
            DateTime? currentMin = null;
            DateTime? currentMax = null;

            for (int i = 0; i < values.Length; i++)
            {
                if (currentMin == null || currentMax == null)
                    currentMin = currentMax = values[i];
                else
                {
                    currentMin = values[i] < (DateTime)currentMin ? values[i] : (DateTime)currentMin;
                    currentMax = values[i] > (DateTime)currentMax ? values[i] : (DateTime)currentMax;
                }
            }

            return new CsvValue(
                            currentMin == currentMax ? new string[1] { CsvExtensions.SerializeObject(currentMin).Item2 } :
                            new string[2] { CsvExtensions.SerializeObject(currentMin).Item2, CsvExtensions.SerializeObject(currentMax).Item2 },
                            value.ColumnIndex, value.ColumnType, value.IsList, true);
        }

        public static CsvValue ToStep(CsvValue value, double stepInterval)
        {
            if (value.ColumnType != CsvType.Timestamp) throw new ArgumentException("Column type is not Timestamp", nameof(value));
            DateTime[] values = value.ToDateTimeList();
            Array.Sort(values);

            List<DateTime> steps = new();
            DateTime? lastValue = null;

            for (int i = 0; i < values.Length; i++)
            {
                if (lastValue == null)
                    lastValue = values[i];
                else if (Math.Abs(((DateTime)lastValue).Ticks - values[i].Ticks) >= stepInterval) // value pulled away
                {
                    steps.Add((DateTime)lastValue);
                    lastValue = values[i];
                }
            }

            if (lastValue != null)
            {
                steps.Add((DateTime)lastValue);
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
