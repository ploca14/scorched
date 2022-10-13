using System.Collections.Generic;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public interface IDimensions<out T>
    {
        T Dimensions { get; }
    }

    public static class DimensionsExtensions
    {
        public static IEnumerable<Vector2Int> AllCoordinates(this IDimensions<Vector2Int> that)
        {
            for (var y = 0; y < that.Dimensions.y; y++)
            for (var x = 0; x < that.Dimensions.x; x++)
                yield return new Vector2Int(x, y);
        }
    }
}