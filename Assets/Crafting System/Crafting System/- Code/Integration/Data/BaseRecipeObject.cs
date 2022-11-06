using System.Collections.Generic;

namespace Polyperfect.Crafting.Integration
{
    /// <summary>
    ///     All recipe types inherit from this by default.
    /// </summary>
    public abstract class BaseRecipeObject : BaseObjectWithID
    {
        public abstract IEnumerable<ItemStack> Ingredients { get; }
        public abstract IEnumerable<ItemStack> Outputs { get; }
    }
}