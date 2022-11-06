using System;
using System.Collections.Generic;
using Polyperfect.Crafting.Framework;

namespace Polyperfect.Crafting.Integration
{
    public class SpecifiedDataAccessorLookup<KEY> : IReadOnlyDataAccessorLookup<KEY>
    {
        readonly Dictionary<Type, Func<object>> accessors = new Dictionary<Type, Func<object>>();

        public IReadOnlyDictionary<KEY, VALUE> GetReadOnlyAccessor<VALUE>()
        {
            return (IReadOnlyDictionary<KEY, VALUE>) accessors[typeof(VALUE)]();
        }

        public void SetAccessor<VALUE>(Func<IReadOnlyDictionary<KEY, VALUE>> accessor)
        {
            accessors[typeof(VALUE)] = accessor;
        }
    }
}