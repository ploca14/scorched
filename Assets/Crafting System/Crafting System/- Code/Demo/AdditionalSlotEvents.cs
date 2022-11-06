using System;
using Polyperfect.Common;
using Polyperfect.Crafting.Integration;
using UnityEngine;
using UnityEngine.Events;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(ItemSlotComponent))]
    public class AdditionalSlotEvents:PolyMono
    {
        public override string __Usage => "Triggers events when the attached item slot updates.";

        [Serializable]
        public class FloatEvent : UnityEvent<float> { }

        public FloatEvent FillAmountAsFraction;
        ItemSlotComponent slot;

        void Awake()
        {
            slot = GetComponent<ItemSlotComponent>();
            slot.Changed += HandleChange;
        }

        void HandleChange()
        {
            FillAmountAsFraction?.Invoke(slot.Peek().Value/(float)slot.MaximumCapacity);
        }
    }
}