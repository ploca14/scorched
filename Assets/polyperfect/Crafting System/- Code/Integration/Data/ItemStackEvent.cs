using System;
using UnityEngine.Events;

namespace Polyperfect.Crafting.Integration
{
    [Serializable]
    public class ItemStackEvent : UnityEvent<ItemStack>
    {
    }
}