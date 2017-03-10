using System;
using System.Linq;

namespace LibP2P.Utilities.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] Slice<T>(this T[] array, int offset, int? count = null)
        {
            if (offset < 0 || offset >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            if (count.HasValue && count.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            var result = new T[Math.Min(count ?? array.Length - offset, array.Length - offset)];
            Array.Copy(array, offset, result, 0, result.Length);
            return result;
        }

        public static T[] Append<T>(this T[] array, params T[] items)
        {
            var result = new T[array.Length + items.Length];
            Array.Copy(array, 0, result, 0, array.Length);
            for (var i = 0; i < items.Length; i++)
                result[array.Length + i] = items[i];
            return result;
        }

        public static int Copy<T>(this T[] src, T[] dst, int offset = 0, int? count = null)
        {
            var actual = Math.Min(src.Length, count ?? dst.Length - offset);
            Array.Copy(src, 0, dst, offset, actual);
            return actual;
        }

        public static int IndexOf<T>(this T[] array, T item)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(item))
                    return i;
            }

            return -1;
        }

        public static int LastIndexOf<T>(this T[] array, T item)
        {
            for (var i = array.Length - 1; i >= 0; i--)
            {
                if (array[i].Equals(item))
                    return i;
            }

            return -1;
        }
    }
}
