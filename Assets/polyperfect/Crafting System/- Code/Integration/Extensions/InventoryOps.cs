using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Assertions;

namespace Polyperfect.Crafting.Integration
{
    public static class InventoryOps
    {
        /// <summary>
        ///     Removes the provided item stack from any of the provided collection.
        /// </summary>
        public static ItemStack ExtractPossibleFromCollection(IList<ItemStack> source, ItemStack extract)
        {
            var ret = new ItemStack(extract.ID, 0);
            var remaining = extract.Value;
            for (var i = 0; i < source.Count; i++)
            {
                if (source[i].ID != extract.ID)
                    continue;

                var extracted = Mathf.Min(remaining, source[i].Value);
                remaining -= extracted;
                source[i] = new ItemStack(source[i].ID, source[i].Value - extracted);
                ret.Value += extracted;
                if (ret.Value >= extract.Value)
                    break;
            }

            return ret;
        }

        /// <summary>
        ///     If the provided ItemStack can be extracted in its entirety from the source collection.
        /// </summary>
        public static bool CanCompletelyExtractFromCollection(IEnumerable<ItemStack> source, ItemStack extract)
        {
            var remaining = extract.Value;
            foreach (var t in source)
            {
                if (t.ID != extract.ID)
                    continue;

                var extracted = Mathf.Min(remaining, t.Value);
                remaining -= extracted;
                if (remaining <= 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     A look at what you'd be able to extract from the entirety of the source collection, of the provided intended extract.
        /// </summary>
        public static ItemStack PeekAtExtractFromCollection(IEnumerable<ItemStack> source, ItemStack extract)
        {
            var ret = new ItemStack(extract.ID, 0);
            var remaining = extract.Value;
            foreach (var t in source)
            {
                if (t.ID != extract.ID)
                    continue;

                var extracted = Mathf.Min(remaining, t.Value);
                remaining -= extracted;
                ret.Value += extracted;
                if (ret.Value >= extract.Value)
                    break;
            }

            return ret;
        }

        /// <summary>
        ///     Extracts all that can be from the source inventory.
        /// </summary>
        public static ItemStack ExtractPossibleFromCollection<T>(IEnumerable<T> source, ItemStack toExtract) where T : IExtract<Quantity, ItemStack>
        {
            var ret = new ItemStack(toExtract.ID, 0);
            var remaining = toExtract.Value;
            var extracted = new ItemStack(toExtract.ID, 0);
            foreach (var extractFrom in source)
            {
                if (!extractFrom.Peek().ID.Equals(extracted.ID))
                    continue;
                var currentExtraction = extractFrom.ExtractAmount(remaining);
                remaining -= currentExtraction.Value;
                extracted.Value += currentExtraction.Value;
                if (remaining <= 0)
                    break;
                ret.Value += extracted.Value;
            }


            return ret;
        }

        /// <summary>
        ///     Extracts all that can be from the source inventory.
        /// </summary>
        public static List<ItemStack> ExtractPossibleFromCollection<T>(IEnumerable<T> source, IEnumerable<ItemStack> toExtract) where T : IExtract<Quantity, ItemStack>
        {
            //note: using list instead of IEnumerable to keep multiple enumeration from extracting multiple times.
            var list = new List<ItemStack>();
            foreach (var extract in toExtract)
            {
                var remaining = extract.Value;
                var extracted = new ItemStack(extract.ID, 0);
                foreach (var extractFrom in source)
                {
                    if (!extractFrom.Peek().ID.Equals(extracted.ID))
                        continue;
                    var currentExtraction = extractFrom.ExtractAmount(remaining);
                    remaining -= currentExtraction.Value;
                    extracted.Value += currentExtraction.Value;
                    if (remaining <= 0)
                        break;
                }

                if (extracted.Value > 0)
                    list.Add(extracted);
            }

            return list;
        }

        /// <summary>
        ///     If the extracted collection can be gleaned from the entirety of the given source.
        /// </summary>
        public static bool CanExtractCompletelyFromCollection(IEnumerable<IExtract<Quantity, ItemStack>> source, IEnumerable<ItemStack> toExtract)
        {
            var copy = source.Select(i => i.Peek()).ToList();
            return toExtract.Select(i => ExtractPossibleFromCollection(copy, i)).All(leftover => leftover.Value <= 0);
        }

        /// <summary>
        ///     Tries to fully extract a collection from another, otherwise throws an error.
        /// </summary>
        /// <exception cref="Exception">If not everything can be extracted.</exception>
        public static List<ItemStack> ExtractCompletelyFromCollection(IEnumerable<IExtract<Quantity, ItemStack>> source, IEnumerable<ItemStack> toExtract)
        {
            var list = new List<ItemStack>();
            var enumeratedSource = source.ToArray();
            foreach (var extract in toExtract)
            {
                var remaining = extract.Value;
                var totalExtracted = new ItemStack(extract.ID, 0);
                foreach (var sourceSlot in enumeratedSource)
                {
                    if (!sourceSlot.Peek().ID.Equals(totalExtracted.ID))
                        continue;
                    var currentExtraction = sourceSlot.ExtractAmount(remaining);
                    remaining -= currentExtraction.Value;
                    totalExtracted.Value += currentExtraction.Value;
                    if (remaining <= 0)
                        break;
                }

                if (remaining > 0)
                    throw new Exception($"Unable to extract {extract.ID} from collection completely. {remaining} remained of {extract.Value}");
                if (totalExtracted.Value > 0)
                    list.Add(totalExtracted);
            }

            return list;
        }

        /// <summary>
        ///     If a collection can be inserted in its entirety.
        /// </summary>
        public static bool CanInsertCollectionCompletely<T>(IEnumerable<ItemStack> source, IEnumerable<T> destination) where T : IInsert<ItemStack>, IPeek<ItemStack>
        {
            return RemainderAfterInsertCollection(source, destination).Count == 0;
        }

        public static void InsertCompletelyIntoCollection<T>(IEnumerable<ItemStack> source, IEnumerable<T> destination) where T : IInsert<ItemStack>, IPeek<ItemStack>
        {
            var list = InsertPossible(source, destination);
            Assert.IsFalse(list.Any());
        }

        public static ItemStack RemainderAfterInsert<T>(ItemStack source, IEnumerable<T> destination) where T : IInsert<ItemStack>, IPeek<ItemStack>
        {
            foreach (var item in destination)
            {
                source = item.RemainderIfInserted(source);
                if (source.IsDefault())
                    break;
            }

            return source;
        }

        /// <summary>
        ///     What would remain after inserting all possible of the given collection
        /// </summary>
        public static List<ItemStack> RemainderAfterInsertCollection<T>(IEnumerable<ItemStack> source, IEnumerable<T> destination)
            where T : IInsert<ItemStack>, IPeek<ItemStack>
        {
            //iterate twice. First tries existing matches, next fills empties
            var list = source.ToList();
            for (var si = 0; si < list.Count; si++)
            {
                var presentAmount = list[si];
                if (presentAmount.Value <= 0)
                    continue;
                foreach (var slot in destination)
                {
                    if (!slot.Peek().ID.Equals(presentAmount.ID))
                        continue;
                    presentAmount = slot.RemainderIfInserted(presentAmount);
                }

                list[si] = presentAmount;
            }

            for (var si = 0; si < list.Count; si++)
            {
                var presentAmount = list[si];
                foreach (var insertInto in destination)
                    presentAmount = insertInto.RemainderIfInserted(presentAmount);
                list[si] = presentAmount;
            }

            for (var i = list.Count - 1; i >= 0; i--)
                if (list[i].Value <= 0)
                    list.RemoveAt(i);

            return list;
        }

        /// <summary>
        ///     Inserts items into the destination. Does not remove them from the source.
        /// </summary>
        /// <returns>Leftovers</returns>
        public static List<ItemStack> InsertPossible<T>(IEnumerable<ItemStack> source, IEnumerable<T> destination)
            where T : IInsert<ItemStack>, IPeek<ItemStack>
        {
            var list = new List<ItemStack>(); //list to prevent multiple enumeration
            var destArray = destination as T[] ?? destination.ToArray();
            foreach (var itemStack in source)
            {
                list.Add(InsertPossible(itemStack, destArray));
            }

            for (var i = list.Count - 1; i >= 0; i--)
                if (list[i].Value <= 0)
                    list.RemoveAt(i);

            return list;
        }

        /// <summary>
        ///     Inserts the ItemStack into the destination, returning remainder.
        /// </summary>
        public static ItemStack InsertPossible<T>(ItemStack source, IEnumerable<T> destination) where T : IInsert<ItemStack>, IPeek<ItemStack>
        {
            //iterate twice. First tries existing matches, next fills empties
            if (source.Value <= 0)
                return default;

            var enumerated = destination as T[] ?? destination.ToArray();
            foreach (var slot in enumerated)
            {
                if (!slot.Peek().ID.Equals(source.ID))
                    continue;
                source = slot.InsertPossible(source);
            }

            foreach (var peek in enumerated)
                source = peek.InsertPossible(source);
            return source;
        }

        /// <summary>
        ///     Multiplies each member by some quantity.
        /// </summary>
        public static IEnumerable<ItemStack> Multiply(this IEnumerable<ItemStack> that, Quantity factor)
        {
            return that.Select(itemStack => new ItemStack(itemStack.ID, itemStack.Value * factor));
        }
    }
}