using System.Collections.Generic;
using System.Linq;
using Polyperfect.Crafting.Framework;

namespace Polyperfect.Crafting.Integration
{
    /// <summary>
    /// Creates collections of items at once.
    /// </summary>
    public class MultiItemFactory : IFactory<Quantity, IEnumerable<ItemStack>>
    {
        readonly IEnumerable<ItemStack> _toCreate;

        public MultiItemFactory(IEnumerable<ItemStack> toCreate)
        {
            _toCreate = toCreate;
        }

        public IEnumerable<ItemStack> Create(Quantity input)
        {
            foreach (var sourceItem in _toCreate.Where(i => !i.IsDefault())) 
                yield return new ItemStack(sourceItem.ID, sourceItem.Value * input);
        }
    }
}