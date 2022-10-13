using System;
using System.Runtime.CompilerServices;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    /// <summary>
    ///     Simple universal item stack. Used by most builtin things in the Crafting System.
    /// </summary>
    [Serializable]
    public struct ItemStack : IValueAndID<Quantity>, IMultiplicable<Quantity, ItemStack>, IEquatable<ItemStack>
    {
        [SerializeField] RuntimeID itemID;
        [SerializeField] Quantity quantity;
        public RuntimeID ID => itemID;

        public Quantity Value
        {
            get => quantity;
            set => quantity = value;
        }

        public ItemStack(RuntimeID id, int quantity)
        {
            this.itemID = id;
            this.quantity = quantity;
        }

        public ItemStack Multiply(Quantity input)
        {
            return new ItemStack(ID, Value * input);
        }

        public override string ToString()
        {
            return $"ID:{ID} Amount:{Value}";
        }

        public bool Equals(ItemStack other)
        {
            return (ID.Equals(other.ID) || Value.Equals(default)) && Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is ItemStack other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ID.GetHashCode() * 397) ^ Value.GetHashCode();
            }
        }

        public static bool operator ==(ItemStack lhs, ItemStack rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ItemStack lhs, ItemStack rhs)
        {
            return !lhs.Equals(rhs);
        }
    }
}