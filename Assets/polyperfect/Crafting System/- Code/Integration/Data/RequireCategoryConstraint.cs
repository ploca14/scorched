using System;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public class RequireCategoryConstraint : IProcessor<ItemStack>
    {
        readonly IItemWorld _world;
        readonly Action _onFailure;
        RuntimeID _category;

        public RequireCategoryConstraint(IItemWorld world, RuntimeID category ,Action onFailure = default)
        {
            _category = category;
            _world = world;
            _onFailure = onFailure;
        }

        public ItemStack Process(ItemStack input)
        {
            if (input.IsDefault())
                return default;
            if (input.ID==_category||_world.CategoryContains(_category,input.ID))
                return input;
            
            _onFailure?.Invoke();
            return default;
        }
    }
}