using System.Collections.Generic;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine.Assertions;

namespace Polyperfect.Crafting.Integration
{
    public static class ItemWorldExtensions
    {
        public static IReadOnlyDictionary<RuntimeID, string> GetNameLookup(this IItemWorld that)
        {
            var accessor = that.GetReadOnlyAccessor<string>(StaticCategories.Names);
            Assert.IsTrue(accessor!=null);
            return accessor;
        }

        public static string GetName(this IItemWorld that,RuntimeID id) =>
            that.GetNameLookup()[id];

        public static RuntimeID GetIDSlow(this IItemWorld that, string name)
        {
            var accessor = that.GetNameLookup();
            foreach (var item in accessor)
            {
                if (item.Value == name)
                    return item.Key;
            }

            throw new KeyNotFoundException();
        }


        public static RuntimeID GetArchetypeFromInstance(this IItemWorld that, RuntimeID instancedItem)
        {
            return that.GetReadOnlyAccessor<RuntimeID>(StaticCategories.Archetypes)[instancedItem];
        }

        public static bool IsInstance(this IItemWorld that, RuntimeID id)
        {
            return that.CategoryContains(StaticCategories.Archetypes,id);
        }
        
        public static T GetInstanceData<T>(this IItemWorld that, RuntimeID category, RuntimeID instance)
        {
            return that.GetReadOnlyAccessor<T>(category).GetDataOrDefault(instance);
        }
    }
}