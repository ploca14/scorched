using System.Collections.Generic;

namespace Polyperfect.Common
{
    public static class CollectionExtensions
    {
        public static int IndexOf<T>(this IReadOnlyList<T> that, T val)
        {
            for (var i = 0; i < that.Count; i++)
                if (that[i].Equals(val))
                    return i;

            return -1;
        }

        public static VALUE GetDataOrDefault<KEY, VALUE>(this IReadOnlyDictionary<KEY, VALUE> that, KEY key)
        {
            if (key == null) return default;

            that.TryGetValue(key, out var ret);
            return ret;
        }

        public static VALUE GetDataOrDefault<KEY, VALUE>(this IDictionary<KEY, VALUE> that, KEY key)
        {
            if (key == null) return default;

            that.TryGetValue(key, out var ret);
            return ret;
        }
        

        public static IEnumerable<VALUE> Empty<VALUE>()
        {
            yield break;
        }
    }
}