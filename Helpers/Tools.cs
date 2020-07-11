using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UvA.Connectors.Canvas.Helpers
{
    static class Tools
    {
        public static T[] GetEnumEntries<T>() => (T[])Enum.GetValues(typeof(T));

        public static IEnumerable<T[]> Split<T>(this IEnumerable<T> input, int count)
        {
            var total = input.Count();
            var i = 0;
            while (i * count < total)
                yield return input.Skip((i++) * count).Take(count).ToArray();
        }
    }
}
