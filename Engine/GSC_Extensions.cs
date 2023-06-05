using System.Collections.Generic;
using UnityEngine;

namespace GSC_Engine
{
    public static class GSC_Extensions
    {
        public static void MinOf(this int val, int value) => val = Mathf.Min(val, value); 
        public static void MaxOf(this int val, int value) => val = Mathf.Max(val, value);

        public static bool IsNullOrEmpty<T>(this T[] arr) => arr is null || arr.Length == 0;

        public static bool IsNullOrEmpty<T>(this List<T> list) => list is null || list.Count == 0;

        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void InsertFromTop<T>(this List<T> list, int positionFromTop, params T[] items)
        {
            int index = list.Count - positionFromTop;
            if (index >= 0 && index <= list.Count)
            {
                list.InsertRange(index, items);
            }
        }

        public static List<T> RemoveFromTop<T>(this List<T> list, int positionFromTop, int count)
        {
            int index = list.Count - positionFromTop - 1;
            if (index >= 0 && index < list.Count && count > 0)
            {
                int endIndex = index + count;
                endIndex = Mathf.Min(endIndex, list.Count);
                List<T> removedItems = list.GetRange(index, endIndex - index);
                list.RemoveRange(index, endIndex - index);
                return removedItems;
            }
            else return null;
        }

        public static void InsertFromBottom<T>(this List<T> list, int positionFromBottom, params T[] items)
        {
            int index = list.Count - positionFromBottom;
            if (index >= 0 && index <= list.Count)
            {
                list.InsertRange(index, items);
            }
        }

        public static List<T> RemoveFromBottom<T>(this List<T> list, int positionFromBottom, int count)
        {
            int index = list.Count - positionFromBottom - 1;
            if (index >= 0 && index < list.Count)
            {
                if (count > list.Count - index) count = list.Count - index;
                List<T> removedItems = list.GetRange(index, count);
                list.RemoveRange(index, count);
                return removedItems;
            }
            return null;
        }
    }
 
}