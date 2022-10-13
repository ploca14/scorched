using System.Collections.Generic;
using UnityEngine;

namespace Polyperfect.Crafting.Framework
{
    /// <summary>
    ///     Compares Int-containing item collections using cumulative item counts regardless of position
    /// </summary>
    /// <typeparam name="ITEM">The type with amount data.</typeparam>
    public class AnyPositionItemQuantitySatisfier<ITEM> : ISatisfier<IEnumerable<ITEM>, Quantity> where ITEM : IValueAndID<Quantity>
    {
        public Quantity SatisfactionWith(IEnumerable<ITEM> requirements, IEnumerable<ITEM> supplied)
        {
            var required = new Dictionary<RuntimeID, int>();
            var has = new Dictionary<RuntimeID, int>();
            foreach (var item in requirements)
            {
                if (item.Value <= 0)
                    continue;

                var itemID = item.ID;
                if (!required.ContainsKey(itemID))
                {
                    required.Add(itemID, 0);
                    has.Add(itemID, 0);
                }

                required[itemID] += item.Value;
            }

            foreach (var item in supplied)
            {
                var itemID = item.ID;
                if (has.ContainsKey(itemID))
                    has[itemID] += item.Value;
            }

            var minQuotient = float.MaxValue;
            var assigned = false;
            foreach (var item in required)
            {
                assigned = true;
                minQuotient = Mathf.Min(minQuotient, has[item.Key] / (float) required[item.Key]);
            }

            return (int) (assigned ? minQuotient : 0f);
        }
    }
}