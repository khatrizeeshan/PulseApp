using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulseApp.Helpers
{
    public static class DictionaryHelper
    {
        public static int? GetValueOrNull(this Dictionary<int, int> dictionary, int key)
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key];
            else
                return null;
        }
    }
}
