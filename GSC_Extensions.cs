using System;
using System.Collections.Generic;


namespace GSC_Engine
{
    public static class GSC_Extensions
    {
        public static bool IsNullOrEmpty<T>(this T[] arr) => arr is null || arr.Length == 0;
        public static bool IsNullOrEmpty<T>(this List<T> list) => list is null || list.Count == 0;
        public static T2 GetValueOrDefault<T1,T2>(this Dictionary<T1,T2> dictionary, T1 Key, T2 DefaultValue)
        {
            return (dictionary.TryGetValue(Key, out T2 value)) ? value : DefaultValue;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
 
}