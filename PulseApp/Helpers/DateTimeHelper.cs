using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulseApp.Helpers
{
    public static class DateTimeHelper
    {
        public static string GetMonthName(int month)
        {
            return new DateTime(2020, month, 1).ToString("MMMM");
        }
    }
}
