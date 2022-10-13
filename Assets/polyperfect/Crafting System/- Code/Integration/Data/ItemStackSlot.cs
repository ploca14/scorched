using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    /// <summary>
    ///     Simple slots that can contain items with amounts.
    /// </summary>
    public class ItemStackSlot : ISlot<Quantity, ItemStack>
    {
        Quantity amount;
        RuntimeID id;

        public ItemStack RemainderIfInserted(ItemStack toInsert)
        {
            var current = Peek();
            if (current.ID.IsDefault() || current.ID.Equals(toInsert.ID))
                return default;
            return toInsert;
        }

        public ItemStack InsertPossible(ItemStack toInsert)
        {
            if (toInsert.Value <= 0)
                return default;
            var current = Peek();
            if (!current.ID.IsDefault() && !toInsert.ID.Equals(current.ID))
                return toInsert;
            id = toInsert.ID;
            amount += toInsert.Value;
            return default;
        }

        public ItemStack ExtractAll()
        {
            var ret = new ItemStack(id, amount);
            id = default;
            amount = 0;
            return ret;
        }

        public bool CanExtract()
        {
            return !id.IsDefault();
        }

        public ItemStack Peek()
        {
            return new ItemStack(id, amount);
        }

        public ItemStack ExtractAmount(Quantity arg)
        {
            var ret = new ItemStack(id, Mathf.Min(arg, amount));
            amount -= ret.Value;
            if (amount == 0)
                id = default;
            return ret;
        }

        public ItemStack Peek(Quantity arg)
        {
            return new ItemStack(id, amount);
        }
    }
}