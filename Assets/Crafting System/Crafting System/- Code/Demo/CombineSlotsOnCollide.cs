using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(ItemSlotComponent))]
    public class CombineSlotsOnCollide : PolyMono
    {
        ItemSlotComponent _slot;
        public override string __Usage => $"If stackable, causes the contents of the attached {nameof(ItemSlotComponent)} to merge with those it comes in contact with.\nOlder instantiations take priority.";
        public bool OtherNeedsCombineScript = true;
        
        void Awake() => _slot = GetComponent<ItemSlotComponent>();

        void OnCollisionEnter(Collision other)
        {
            if (other.collider.GetInstanceID() < _slot.GetInstanceID())
                return;
            if (OtherNeedsCombineScript && !other.collider.GetComponentInParent<CombineSlotsOnCollide>())
                return;
            var otherSlot = other.collider.GetComponentInParent<IInsert<ItemStack>>();
            if (otherSlot != null)
                TryCombine(otherSlot);
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.GetInstanceID() < _slot.GetInstanceID())
                return;
            if (OtherNeedsCombineScript && !other.GetComponentInParent<CombineSlotsOnCollide>())
                return;
            var otherSlot = other.GetComponentInParent<IInsert<ItemStack>>();
            if (otherSlot!=null)
                TryCombine(otherSlot);
        }
        void TryCombine(IInsert<ItemStack> otherSlot)
        {
            if (otherSlot.CanInsertCompletely(_slot.Peek()))
                otherSlot.InsertCompletely(_slot.ExtractAll());
        }
    }
}