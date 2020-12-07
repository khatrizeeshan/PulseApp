using Google.Protobuf.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PulseApp.Helpers
{
    public static class DictionaryHelper
    {
        public static int? GetValueOrNull(this MapField<int, int> field, int key)
        {
            if (field.ContainsKey(key))
                return field[key];
            else
                return null;
        }
    }
}
