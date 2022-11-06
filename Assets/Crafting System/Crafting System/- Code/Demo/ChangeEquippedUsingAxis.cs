using Polyperfect.Common;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(EquippedSlot))]
    public class ChangeEquippedUsingAxis : PolyMono
    {
        [SerializeField] ChildSlotsInventory TargetInventory;

        public override string __Usage => "Changes the equipped slot using a particular axis.";
        public string Axis = "Mouse ScrollWheel";
        int currentIndex = 0;
        int lastIndex=-1;
        EquippedSlot equipped;
        void Awake()
        {
            equipped = GetComponent<EquippedSlot>();
        }

        void Update()
        {
            var cur = Input.GetAxisRaw(Axis);

            if (cur > 0)
                currentIndex++;
            else if (cur < 0)
                currentIndex--;

            var slots = TargetInventory.SlotList;
            
            currentIndex = (currentIndex + slots.Count) % slots.Count;
            if (slots.Count <= 0)
                equipped.Slot = null;
            else if (currentIndex != lastIndex)
                equipped.Slot = slots[currentIndex];
            

            lastIndex = currentIndex;
        }
    }
}