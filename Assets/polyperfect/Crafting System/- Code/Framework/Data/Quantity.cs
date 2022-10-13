using System;

namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     A value used by most things in the pack to represent an amount of something, to potentially allow for changing the
    ///     internal type if necessary.
    /// </summary>
    [Serializable]
    public struct Quantity : IComparable<Quantity>, IEquatable<Quantity>
    {
        public int Value;

        public Quantity(int value)
        {
            Value = value;
        }

        public int CompareTo(Quantity other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(Quantity other)
        {
            return Value == other.Value;
        }

        public static implicit operator int(Quantity quantity)
        {
            return quantity.Value;
        }

        public static implicit operator Quantity(int amount)
        {
            return new Quantity(amount);
        }

        public override bool Equals(object obj)
        {
            return obj is Quantity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}