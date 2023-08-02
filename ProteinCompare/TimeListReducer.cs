using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public class TimeListReducer : ICsvListReducer
    {
        public static CsvValue ToUnique(CsvValue value)
        {
            return new CsvValue(value.Values.Distinct().ToArray(), value.ColumnIndex, value.ColumnType, value.IsList, false); // still valid list so not actually reduced
        }

        public static CsvValue ToRange(CsvValue value, double jumpInterval)
        {
            if (value.ColumnType != CsvType.Time) throw new ArgumentException("Column type is not Time", nameof(value));
            TimeOnly[] values = value.ToTimeList();
            Array.Sort(values);

            List<(TimeOnly, TimeOnly)> ranges = new(); // min, max
            TimeOnly? currentMin = null;
            TimeOnly? currentMax = null;

            for (int i = 0; i < values.Length; i++)
            {
                if (currentMin == null || currentMax == null)
                    currentMin = currentMax = values[i];
                else
                {
                    TimeOnly min = values[i] < (TimeOnly)currentMin ? values[i] : (TimeOnly)currentMin;
                    TimeOnly max = values[i] > (TimeOnly)currentMax ? values[i] : (TimeOnly)currentMax;

                    if (Math.Abs(min.Ticks - max.Ticks) >= jumpInterval) // value pulled away
                    {
                        ranges.Add(((TimeOnly)currentMin, (TimeOnly)currentMax));
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
                ranges.Add(((TimeOnly)currentMin, (TimeOnly)currentMax));
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
            if (value.ColumnType != CsvType.Time) throw new ArgumentException("Column type is not Time", nameof(value));
            TimeOnly[] values = value.ToTimeList();
            TimeOnly? currentMin = null;
            TimeOnly? currentMax = null;

            for (int i = 0; i < values.Length; i++)
            {
                if (currentMin == null || currentMax == null)
                    currentMin = currentMax = values[i];
                else
                {
                    currentMin = values[i] < (TimeOnly)currentMin ? values[i] : (TimeOnly)currentMin;
                    currentMax = values[i] > (TimeOnly)currentMax ? values[i] : (TimeOnly)currentMax;
                }
            }

            return new CsvValue(
                            currentMin == currentMax ? new string[1] { CsvExtensions.SerializeObject(currentMin).Item2 } :
                            new string[2] { CsvExtensions.SerializeObject(currentMin).Item2, CsvExtensions.SerializeObject(currentMax).Item2 },
                            value.ColumnIndex, value.ColumnType, value.IsList, true);
        }

        public static CsvValue ToStep(CsvValue value, double stepInterval)
        {
            if (value.ColumnType != CsvType.Time) throw new ArgumentException("Column type is not Time", nameof(value));
            TimeOnly[] values = value.ToTimeList();
            Array.Sort(values);

            List<TimeOnly> steps = new();
            TimeOnly? lastValue = null;

            for (int i = 0; i < values.Length; i++)
            {
                if (lastValue == null)
                    lastValue = values[i];
                else if (Math.Abs(((TimeOnly)lastValue).Ticks - values[i].Ticks) >= stepInterval) // value pulled away
                {
                    steps.Add((TimeOnly)lastValue);
                    lastValue = values[i];
                }
            }

            if (lastValue != null)
            {
                steps.Add((TimeOnly)lastValue);
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
