using System;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public class FuncProcessor : IProcessor<ItemStack>
    {
        readonly Func<ItemStack, ItemStack> _funcs;

        public FuncProcessor(Func<ItemStack, ItemStack> funcs) => _funcs = funcs;

        public ItemStack Process(ItemStack input)
        {
            var itemStack = _funcs(input);
            return itemStack;
        }
    }
}