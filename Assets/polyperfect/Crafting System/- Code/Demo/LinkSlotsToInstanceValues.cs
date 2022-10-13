using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(ItemSlotComponent))]
    public class LinkSlotsToInstanceValues : ItemUserBase
    {
        public override string __Usage => $"Makes the {nameof(AffectedSlot)}'s fill amount get stored in the instance data of the item in the {nameof(InstanceSlot)}.\nAlso makes {nameof(AffectedSlot)} respect the capacity defined in the {nameof(MaximumSource)} data of the item in {nameof(InstanceSlot)}.";

        [HighlightNull] public CategoryWithInt MaximumSource;
        [HighlightNull] public CategoryWithInt CurrentSource;

        [HighlightNull] [SerializeField] ItemSlotComponent InstanceSlot;
        [HighlightNull] [SerializeField] ItemSlotComponent AffectedSlot;

        [HighlightNull] [SerializeField] BaseItemObject ItemType;

        void Awake()
        {
            InstanceSlot.Changed += () =>
            {
                AffectedSlot.MaximumCapacity = World.GetReadOnlyAccessor<int>(MaximumSource).GetDataOrDefault(InstanceSlot.Peek().ID);
                AffectedSlot.InsertCompletely(new ItemStack(ItemType.ID,World.GetReadOnlyAccessor<int>(CurrentSource).GetDataOrDefault(InstanceSlot.Peek().ID)));
            };

            AffectedSlot.Changed += () =>
            {
                if (InstanceSlot.Peek().IsDefault())
                    return;

                World.SetInstanceData(CurrentSource, InstanceSlot.Peek().ID, AffectedSlot.Peek().Value.Value);
            };
        }
    }
}