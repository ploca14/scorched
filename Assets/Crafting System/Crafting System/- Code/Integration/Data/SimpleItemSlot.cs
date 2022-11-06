using Polyperfect.Crafting.Framework;

namespace Polyperfect.Crafting.Integration
{
    public class SimpleItemSlot<T> : IInsert<T>, IExtract<T>
    {
        T item;

        public T ExtractAll()
        {
            var ret = item;
            item = default;
            return ret;
        }

        public bool CanExtract()
        {
            return !Peek().IsDefault();
        }

        public T Peek()
        {
            return item;
        }

        public T RemainderIfInserted(T toInsert)
        {
            return item.IsDefault() ? default : toInsert;
        }

        public T InsertPossible(T toInsert)
        {
            if (!item.IsDefault())
                return toInsert;

            item = toInsert;
            return default;
        }
    }
}