using System;
using System.Collections.Generic;
using UnityEngine;

namespace Polyperfect.Common
{
    public static class IEnumerableExtensions
    {
        public static T MinBy<T>(this IEnumerable<T> that, Func<T, float> selector)
        {
            var min = Mathf.Infinity;
            var ret = default(T);
            foreach (var item in that)
            {
                var val = selector(item);
                if (val < min)
                {
                    min = val;
                    ret = item;
                }
            }
            return ret;
        }
        public static T MaxBy<T>(this IEnumerable<T> that, Func<T, float> selector)
        {
            var max = Mathf.NegativeInfinity;
            var ret = default(T);
            foreach (var item in that)
            {
                var val = selector(item);
                if (val > max)
                {
                    max = val;
                    ret = item;
                }
            }
            return ret;
        }
        
    }
}