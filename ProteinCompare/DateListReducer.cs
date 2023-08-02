using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public class DateListReducer : ICsvListReducer
    {
        public static CsvValue ToUnique(CsvValue value)
        {
            return new CsvValue(value.Values.Distinct().ToArray(), value.ColumnIndex, value.ColumnType, value.IsList, false); // still valid list so not actually reduced
        }

        public static CsvValue ToRange(CsvValue value, double jumpInterval)
        {
            if (value.ColumnType != CsvType.Date) throw new ArgumentException("Column type is not Date", nameof(value));
            DateOnly[] values = value.ToDateList();
            Array.Sort(values);

            List<(DateOnly, DateOnly)> ranges = new(); // min, max
            DateOnly? currentMin = null;
            DateOnly? currentMax = null;

            for (int i = 0; i < values.Length; i++)
            {
                if (currentMin == null || currentMax == null)
                    currentMin = currentMax = values[i];
                else
                {
                    DateOnly min = values[i] < (DateOnly)currentMin ? values[i] : (DateOnly)currentMin;
                    DateOnly max = values[i] > (DateOnly)currentMax ? values[i] : (DateOnly)currentMax;

                    if (Math.Abs(min.ToDateTime(TimeOnly.MinValue).Ticks - max.ToDateTime(TimeOnly.MinValue).Ticks) >= jumpInterval) // value pulled away
                    {
                        ranges.Add(((DateOnly)currentMin, (DateOnly)currentMax));
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
                ranges.Add(((DateOnly)currentMin, (DateOnly)currentMax));
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
            if (value.ColumnType != CsvType.Date) throw new ArgumentException("Column type is not Date", nameof(value));
            DateOnly[] values = value.ToDateList();
            DateOnly? currentMin = null;
            DateOnly? currentMax = null;

            for (int i = 0; i < values.Length; i++)
            {
                if (currentMin == null || currentMax == null)
                    currentMin = currentMax = values[i];
                else
                {
                    currentMin = values[i] < (DateOnly)currentMin ? values[i] : (DateOnly)currentMin;
                    currentMax = values[i] > (DateOnly)currentMax ? values[i] : (DateOnly)currentMax;
                }
            }

            return new CsvValue(
                            currentMin == currentMax ? new string[1] { CsvExtensions.SerializeObject(currentMin).Item2 } :
                            new string[2] { CsvExtensions.SerializeObject(currentMin).Item2, CsvExtensions.SerializeObject(currentMax).Item2 },
                            value.ColumnIndex, value.ColumnType, value.IsList, true);
        }

        public static CsvValue ToStep(CsvValue value, double stepInterval)
        {
            if (value.ColumnType != CsvType.Date) throw new ArgumentException("Column type is not Date", nameof(value));
            DateOnly[] values = value.ToDateList();
            Array.Sort(values);

            List<DateOnly> steps = new();
            DateOnly? lastValue = null;

            for (int i = 0; i < values.Length; i++)
            {
                if (lastValue == null)
                    lastValue = values[i];
                else if (Math.Abs(((DateOnly)lastValue).ToDateTime(TimeOnly.MinValue).Ticks - values[i].ToDateTime(TimeOnly.MinValue).Ticks) >= stepInterval) // value pulled away
                {
                    steps.Add((DateOnly)lastValue);
                    lastValue = values[i];
                }
            }

            if (lastValue != null)
            {
                steps.Add((DateOnly)lastValue);
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
