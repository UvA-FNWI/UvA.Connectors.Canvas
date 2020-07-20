using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UvA.Connectors.Canvas.Helpers
{
    public static class Extensions
    {
        /// <summary>
        /// Converts a list of objects to a separated string
        /// </summary>
        /// <typeparam name="T">The type of the objects</typeparam>
        /// <param name="list">The list to convert</param>
        /// <param name="displayFunction">A function that gets converts each object to a string</param>
        /// <param name="separator">The separator to use between the objects</param>
        /// <param name="word">An optional word to use for the last object, e.g. 'and'</param>
        /// <returns></returns>
        public static string ToSeparatedString<T>(this IEnumerable<T> list, Func<T, string> displayFunction = null,
            string separator = ", ", string word = null)
        {
            // If no function is specified, just call the ToString method on each object
            if (displayFunction == null)
                displayFunction = d => d == null ? "" : d.ToString();

            StringBuilder builder = new StringBuilder();
            int count = 0;
            foreach (var l in list)
            {
                count++;

                // Append the object
                builder.Append(displayFunction(l));

                // If this is the last object, there is more than one object AND the word parameter is specified, 
                // add the word. Otherwise, add the separator if this is not the last object
                if (word != null && count > 0 && count == list.Count() - 1)
                    builder.Append(" " + word + " ");
                else if (count != list.Count())
                    builder.Append(separator);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Adds an object to a collection
        /// </summary>
        /// <param name="col">A collection of objects</param>
        /// <param name="item">Object to add</param>
        /// <returns></returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> col, T item)
        {
            return col.Concat(new[] { item });
        }
    }
}
