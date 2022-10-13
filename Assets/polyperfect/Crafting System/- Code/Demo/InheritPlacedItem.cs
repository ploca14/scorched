using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using Polyperfect.Crafting.Placement;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(ItemSlotComponent))]
    public class InheritPlacedItem : ItemUserBase, IPlacementPostprocessor
    {
        public override string __Usage => $"Inserts the placed item into the attached {nameof(ItemSlotComponent)}";

        [SerializeField] bool MakePlacedItemIntoInstance;

        public void PostprocessPlacement(in PlacementInfo info)
        {
            if (info.TryGetGenericData(ItemPlacer.PlacedItemStackKey, out ItemStack placedStack))
                GetComponent<ItemSlotComponent>().InsertCompletely(
                    MakePlacedItemIntoInstance && !World.IsInstance(placedStack.ID)
                        ? new ItemStack(World.CreateInstance(placedStack.ID), 1)
                        : placedStack);
        }
    }
}