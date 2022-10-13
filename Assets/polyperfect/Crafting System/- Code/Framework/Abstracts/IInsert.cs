using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Something that can have a T put into it.
    /// </summary>
    public interface IInsert<INPUT>
    {
        /// <returns>Remainder.</returns>
        INPUT RemainderIfInserted(INPUT toInsert);

        /// <returns>Remainder.</returns>
        INPUT InsertPossible(INPUT toInsert);
    }


    public static class InsertExtensions
    {
        public static bool TryInsert<T>(this IInsert<T> that, T toInsert)
        {
            if (!that.CanInsertAny(toInsert))
                return false;
            that.InsertPossible(toInsert);
            return true;
        }

        /// <summary>
        ///     Whether the target can have any portion of the inserted object inserted.
        /// </summary>
        public static bool CanInsertAny<INSERT>(this IInsert<INSERT> that, INSERT toInsert)
        {
            return !that.RemainderIfInserted(toInsert).Equals(toInsert);
        }

        /// <summary>
        ///     Whether the target can fully accept the inserted object.
        /// </summary>
        public static bool CanInsertCompletely<INSERT>(this IInsert<INSERT> that, INSERT toInsert)
        {
            return that.RemainderIfInserted(toInsert).IsDefault();
        }


        /// <summary>
        ///     Inserts possible, but throws an error if unable to insert entirely.
        /// </summary>
        public static void InsertCompletely<INSERT>(this IInsert<INSERT> that, INSERT toInsert)
        {
            var ret = that.InsertPossible(toInsert);
            if (!ret.IsDefault())
                Debug.LogError($"Unable to insert {toInsert} completely. Remainder was {ret}");
        }
    }
}