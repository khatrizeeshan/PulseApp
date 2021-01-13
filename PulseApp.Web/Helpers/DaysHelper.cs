using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulseApp.Helpers
{
    public static class DaysHelper
    {
        public static string GetWeekendNames(string weekends)
        {
            var array = weekends.ToCharArray();
            var list = new List<string>();
            var on = '1';
            for(int i = 0; i < 7; i++)
            {
                if(array[i] == on)
                {
                    list.Add(Enum.GetName((DayOfWeek)i));
                }
            }

            return string.Join(",", list);
        }

        public static string GetName(int day)
        {
           return Enum.GetName((DayOfWeek)day);
        }
    }
}
