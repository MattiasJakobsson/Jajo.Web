using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SuperGlue.Web.Assets
{
    public static class GenericEnumerableExtensions
    {
        public static bool IsEqualTo<T>(this IEnumerable<T> actual, IEnumerable<T> expected)
        {
            var objArray1 = actual.ToArray();
            var objArray2 = expected.ToArray();
            
            if (objArray1.Length != objArray2.Length)
                return false;
            
            return !objArray1.Where((t, index) => !t.Equals(objArray2[index])).Any();
        }

        public static string Join(this string[] values, string separator)
        {
            return string.Join(separator, values);
        }

        public static string Join(this IEnumerable<string> values, string separator)
        {
            return Join(values.ToArray(), separator);
        }

        [DebuggerStepThrough]
        public static IEnumerable<T> Each<T>(this IEnumerable<T> values, Action<T> eachAction)
        {
            foreach (var obj in values)
                eachAction(obj);

            return values;
        }

        [DebuggerStepThrough]
        public static IEnumerable Each(this IEnumerable values, Action<object> eachAction)
        {
            foreach (var obj in values)
                eachAction(obj);

            return values;
        }

        public static void Fill<T>(this IList<T> list, T value)
        {
            if (list.Contains(value))
                return;
            list.Add(value);
        }

        public static void Fill<T>(this IList<T> list, IEnumerable<T> values)
        {
            list.AddRange(values.Where(v => !list.Contains(v)));
        }

        public static IList<T> AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            items.Each(((ICollection<T>)list).Add);
            return list;
        }
    }
}