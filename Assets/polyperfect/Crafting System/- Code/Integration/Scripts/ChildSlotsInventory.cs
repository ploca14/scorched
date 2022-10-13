using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Events;
using SLOT = Polyperfect.Crafting.Framework.ISlot<Polyperfect.Crafting.Framework.Quantity, Polyperfect.Crafting.Integration.ItemStack>;

namespace Polyperfect.Crafting.Integration
{
    public class ChildSlotsInventory : BaseItemStackInventory, IChangeable
    {
        [Serializable]
        class StackAddedEvent : UnityEvent<ItemStack>
        {
        }

        public override string __Usage =>
            $"Gets all child slots, like {nameof(ItemSlotComponent)}s. If {nameof(GrabFromSubInventories)} is enabled, it will ignore slots like Discard Slots that do not belong to other inventories.";

        public IReadOnlyList<SLOT> SlotList => RepresentativeInventory.SlotList;
        SlottedInventory<SLOT> RepresentativeInventory
        {
            get
            {
                TryInit();
                return representativeInventory;
            }
        }

        SlottedInventory<SLOT> representativeInventory;

        [Tooltip("If true, will only get child slots that are already members of other inventories. Useful for avoiding grabbing discard slots for example.")]
        [SerializeField]
        bool GrabFromSubInventories = false;

        bool initted;

        public override void TryInit()
        {
            if (initted)
                return;

            initted = true;
            representativeInventory = new SlottedInventory<SLOT>();
            GrabSlots();
        }

        void GrabSlots()
        {
            RepresentativeInventory.SlotList.Clear();
            if (!GrabFromSubInventories)
            {
                foreach (var slot in GetComponentsInChildren<SLOT>(true))
                {
                    AddSlot(slot);
                }
            }
            else
            {
                foreach (var subInventory in GetComponentsInChildren<BaseItemStackInventory>(true).Where(c => c != this))
                {
                    subInventory.TryInit();
                    foreach (var slot in subInventory.Slots)
                    {
                        AddSlot(slot);
                    }
                }
            }
        }

        protected override ISlottedInventory<SLOT> _slottedInventoryImplementation => RepresentativeInventory;

        public event PolyChangeEvent Changed;

        public void SetInventory(SlottedInventory<SLOT> newInventory)
        {
            if (!RepresentativeInventory.IsDefault())
                UnregisterAll();
            representativeInventory = newInventory;
            RegisterAll();
        }

        public void AddSlot(SLOT slot)
        {
            if (RepresentativeInventory.Slots.Contains(slot))
                return;
            RepresentativeInventory.SlotList.Add(slot);
            if (slot is IChangeable changeable)
                changeable.Changed += Changed;
        }

        public void RemoveSlot(SLOT slot)
        {
            RepresentativeInventory.SlotList.Remove(slot);
            if (slot is IChangeable changeable)
                changeable.Changed -= Changed;
        }

        void RegisterAll()
        {
            foreach (var slot in RepresentativeInventory.Slots.Where(s => s is IChangeable).Cast<IChangeable>())
                slot.Changed += Changed;
        }

        void UnregisterAll()
        {
            foreach (var slot in RepresentativeInventory.Slots.Where(s => s is IChangeable).Cast<IChangeable>())
                slot.Changed -= Changed;
        }
    }
}