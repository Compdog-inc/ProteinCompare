using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteinCompare
{
    public interface ICsvListReducer
    {
        static abstract CsvValue ToUnique(CsvValue value);
        static abstract CsvValue ToRange(CsvValue value, double jumpInterval); // double is the highest resolution used
        static abstract CsvValue ToStep(CsvValue value, double stepInterval);  // double is the highest resolution used
        static abstract CsvValue ToStartEnd(CsvValue value);
    }
}
