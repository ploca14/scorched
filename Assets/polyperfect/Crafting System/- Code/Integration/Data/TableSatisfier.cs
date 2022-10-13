using System.Linq;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public class TableSatisfier<T> : ISatisfier<T, int> where T : IIndexed<Vector2Int, IValueAndID<Quantity>>
    {
        readonly MatchingPositionSatisfier<IValueAndID<Quantity>, IValueAndID<Quantity>, Quantity> _satisfier;

        public TableSatisfier()
        {
            _satisfier = new MatchingPositionSatisfier<IValueAndID<Quantity>, IValueAndID<Quantity>, Quantity>(new QuantityAndIDSatisfier<IValueAndID<Quantity>>(),
                int.MaxValue);
        }

        public int SatisfactionWith(T requirements, T supplied)
        {
            return _satisfier.SatisfactionWith(requirements.Indices.Select(i => requirements[i]), requirements.Indices.Select(i => supplied[i]));
        }
    }
}