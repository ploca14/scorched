using System.Collections.Generic;

namespace Polyperfect.Crafting.Framework
{
    public static class ObjectExtensions
    {
        /// <summary>
        ///     If the value is null or otherwise matches the default type.
        /// </summary>
        public static bool IsDefault<T>(this T that)
        {
            return EqualityComparer<T>.Default.Equals(that, default);
        }

        /// <summary>
        ///     Utility to quickly turn something into a collection;
        /// </summary>
        public static IEnumerable<T> MakeEnumerable<T>(this T that)
        {
            yield return that;
        }
    }
}