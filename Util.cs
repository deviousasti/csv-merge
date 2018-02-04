using System;
using System.Collections.Generic;
using System.Linq;

namespace csv_merge
{
    static class Util
    {
        public static IEnumerable<T> StartWith<T>(this IEnumerable<T> enumerable, T value)
        {
            yield return value;
            foreach (var item in enumerable)
                yield return item;
        }
    }
}
