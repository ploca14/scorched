using System;

namespace Polyperfect.Crafting.Framework
{
    public class ValueAndIDSatisfier<T> : ISatisfier<IValueAndID<T>, bool> where T : struct, IComparable<T>
    {
        public bool SatisfactionWith(IValueAndID<T> requirement, IValueAndID<T> supplied)
        {
            if (requirement == null)
                return true;
            if (supplied == null)
                return false;

            return requirement.ID == supplied.ID && requirement.Value.CompareTo(supplied.Value) <= 0;
        }
    }
}