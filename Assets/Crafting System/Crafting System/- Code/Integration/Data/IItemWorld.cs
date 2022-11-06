using System.Collections.Generic;
using Polyperfect.Crafting.Framework;

namespace Polyperfect.Crafting.Integration
{
    public interface IItemWorld : IReadOnlyDataAccessorLookupWithArg<RuntimeID, RuntimeID>
    {
        bool CategoryContains(RuntimeID category, RuntimeID itemID);
        IEnumerable<RuntimeID> CategoryMembers(RuntimeID categoryID);
        IReadOnlyDictionary<RuntimeID, SimpleRecipe> Recipes { get; }
        RuntimeID CreateInstance(RuntimeID archetype);
        void DeleteInstance(RuntimeID instance);
        void SetInstanceData<T>(RuntimeID category, RuntimeID instance, T data);
    }
}